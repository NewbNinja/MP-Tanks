/*  This file is part of the "Simple IAP System" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS
//Create your obfuscator secrets using Services > In App PurchasingP > Receipt Validation Obfuscator before enabling server receipt validation in the next line,
//or add it to your Project Settings > Player > Other Settings under Scripting Define Symbols to keep this setting even when upgrading and overwriting this file 
//#define RECEIPT_VALIDATION
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SIS.SimpleJSON;

#if SIS_IAP
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
#endif

namespace SIS
{
    /// <summary>
    /// Server-side, remote receipt validation via Receipt Validator service (https://flobuk.com/validator).
    /// Supports getting user inventory, validation on purchase and doing purchase restores.
    /// </summary>
    #if !SIS_IAP
    public class ReceiptValidatorServer : ReceiptValidator
    {
        void Start()
        {
            Debug.LogError("You are using " + GetType() + " but did not set up Unity IAP. Validation code will not compile!");
        }
    }
    #endif

    #if SIS_IAP
	public class ReceiptValidatorServer : ReceiptValidator
    {
        #pragma warning disable 0067,0414
        public static event Action inventoryCallback;
        public static event Action<string, JSONNode> purchaseCallback;

        [Header("General Data")]
        public string appID;
        public string userID = "";

        [Header("User Inventory is not supported on the Free plan.", order = 0)]
        [Header("Please leave it on 'Disabled' if you didn't upgrade.", order = 1)]
        public InventoryRequestType inventoryRequestType = InventoryRequestType.Disabled;

        const string validationEndpoint = "https://flobuk.com/validator/v1/receipt/";
        const string userEndpoint = "https://flobuk.com/validator/v1/user/";

        Dictionary<string, PurchaseResponse> inventory = new Dictionary<string, PurchaseResponse>();
        CrossPlatformValidator localValidator = null;
        const string lastInventoryTimestampKey = "fbrv_inventory_timestamp";
        float lastInventoryTime = -1;
        bool inventoryRequestActive = false;
        int inventoryDelay = 1800; //30 minutes
        #pragma warning restore 0067,0414


        #if !RECEIPT_VALIDATION
        void Start()
        {
            #if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS
                Debug.LogError("You are using " + GetType() + " but did not add the 'RECEIPT_VALIDATION' define to 'Project Settings > Player > Scripting Define Symbols'. Validation code will not compile!");
            #endif
        }
        #endif


        #if RECEIPT_VALIDATION
        //subscribe to IAPManager events
        void Start()
        {
            if (!CanValidate() || !IAPManager.GetInstance())
                return;

            IAPManager.receiptValidationInitializeEvent += OnBillingInitialized;
            IAPManager.receiptValidationPurchaseEvent += Validate;
            IAPManager.restoreTransactionsFinishedEvent += RequestRestore;
        }


        #if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_TVOS)
        public override bool CanValidate()
        {          
            if (Application.platform == RuntimePlatform.Android && StandardPurchasingModule.Instance().appStore != AppStore.GooglePlay)
                return false;

            return true;
        }
		#endif


        void OnBillingInitialized()
        {
            //removing products which do not have a local receipt to begin with
            foreach (Product product in IAPManager.controller.products.all)
            {
                if (DBManager.IsPurchased(product.definition.id) && !product.hasReceipt)
                    RemovePurchase(product.definition.id);
            }

            if (IsLocalValidationSupported())
            {
                #if !UNITY_EDITOR
                    localValidator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
                #endif
            }

            if (inventoryRequestType == InventoryRequestType.Disabled || inventoryRequestType == InventoryRequestType.Manual)
                return;

            RequestInventory();
        }


        /// <summary>
        /// Request inventory from the server, for the user specified as 'userID'.
        /// </summary>
        public void RequestInventory()
        {
            //in case requesting inventory was disabled or limited by delay timing
            if (!CanRequestInventory())
            {
                if (IAPManager.isDebug && inventoryRequestType != InventoryRequestType.Disabled)
                    Debug.LogWarning("Receipt Validator: CanRequestInventory returned false.");

                return;
            }

            //server validation is not supported on this platform, so no inventory is stored either or requests exceeded
            if (IAPManager.GetInstance() == null || IAPManager.controller == null || !IsServerValidationSupported())
            {
                if (IAPManager.isDebug) Debug.LogWarning("Receipt Validator: Inventory Request not supported.");
                return;
            }

            //no purchase detected on this account, RequestInventory call is not necessary and was cancelled
            //if you are sure that this account has purchased products, instruct the user to initiate a restore first
            if(!HasActiveReceipt() && !HasPurchaseHistory())
            {
                if (IAPManager.isDebug) Debug.LogWarning("Receipt Validator: Inventory Request not necessary.");
                return;
            }

            inventoryRequestActive = true;
            StartCoroutine(RequestInventoryRoutine());
        }


        /// <summary>
        /// Request re-validation of all product receipts available in Unity IAP locally.
        /// </summary>
        public void RequestRestore(bool success)
        {
            if (IAPManager.GetInstance() == null || IAPManager.controller == null || !success)
            {
                if (IAPManager.isDebug) Debug.Log("IAPManager is not initialized yet. StoreController is null.");
                return;
            }

            #if UNITY_IOS
			    if (IAPManager.isDebug) Debug.Log("RestoreTransactions result: " + success);
            #else
                StartCoroutine(RequestRestoreRoutine());
            #endif
        }


        //on purchase (Product)
        public override void Validate(Product product)
        {
            if (!product.hasReceipt)
            {
                if (IAPManager.isDebug) Debug.Log("Unable to validate, Product '" + product.definition.storeSpecificId + "' does not have a receipt.");
                return;
            }

            if (IsLocalValidationSupported() && localValidator != null)
            {
                PurchaseState state = LocalValidation(product);
                switch (state)
                {
                    case PurchaseState.Purchased: //continue with server validation
                        break;
                    case PurchaseState.Pending: //do nothing
                        if (IAPManager.isDebug) Debug.Log("Local Validation: Product purchase '" + product.definition.storeSpecificId + "' is pending.");
                        return;
                    case PurchaseState.Failed: //set transaction finished
                        if (IAPManager.isDebug) Debug.Log("Local Validation: Product purchase '" + product.definition.storeSpecificId + "' deemed as invalid.");
                        IAPManager.controller.ConfirmPendingPurchase(product);
                        IAPManager.OnPurchaseFailed("Your Purchase: " + product.metadata.localizedTitle + "\n\nYour transaction failed validation.");
                        return;
                }
            }

            if (IsServerValidationSupported())
            {
                StartCoroutine(RequestPurchaseRoutine(product));
            }
        }


        IEnumerator RequestInventoryRoutine()
        {
            using (UnityWebRequest www = UnityWebRequest.Get(userEndpoint + appID + "/" + userID))
            {
                www.SetRequestHeader("content-type", "application/json");
                yield return www.SendWebRequest();

                //raw JSON response
                JSONNode rawResponse = JSON.Parse(www.downloadHandler.text);
                JSONArray purchaseArray = rawResponse["purchases"].AsArray;

                inventory.Clear();
                for (int i = 0; i < purchaseArray.Count; i++)
                {
                    string productId = IAPManager.GetProductGlobalIdentifier(purchaseArray[i]["data"]["productId"].Value);
                    PurchaseResponse purchase = JsonUtility.FromJson<PurchaseResponse>(purchaseArray[i]["data"].ToString());
                    inventory.Add(productId, purchase);

                    //grant in DBManager
                    if (IsPurchased(productId))
                    {
                        if (!DBManager.IsPurchased(productId))
                        {
                            if (IAPManager.isDebug) Debug.Log("Inventory unlocked product: '" + productId + ".");
                            DBManager.SetPurchase(productId);
                        }
                    }
                    else
                        RemovePurchase(productId);
                }

                //removing products which are not included in retrieved user inventory
                foreach (Product product in IAPManager.controller.products.all)
                {
                    if (DBManager.IsPurchased(product.definition.id) && !inventory.ContainsKey(product.definition.id))
                        RemovePurchase(product.definition.id);
                }

                SetPurchaseHistory();
            }

            lastInventoryTime = Time.realtimeSinceStartup;
            inventoryRequestActive = false;
            inventoryCallback?.Invoke();
        }


        // handles an online verification request and response
        IEnumerator RequestPurchaseRoutine(Product product)
        {
            JSONNode receiptData = JSON.Parse(product.receipt);
            string transactionID = receiptData["TransactionID"].Value;

            #if UNITY_IOS
            IPurchaseReceipt[] receipts = localValidator.Validate(product.receipt);
            foreach (AppleInAppPurchaseReceipt receipt in receipts)
            {
                if (product.definition.id != receipt.productID)
                    continue;

                transactionID = receipt.transactionID;
                break;
            }
            #endif

            ReceiptRequest request = new ReceiptRequest()
            {
                store = receiptData["Store"].Value,
                bid = Application.identifier,
                pid = product.definition.storeSpecificId,
                user = userID,
                type = GetType(product.definition.type),
                receipt = transactionID
            };
            string postData = JsonUtility.ToJson(request);

            JSONNode rawResponse = null;
            bool success = false;
            using (UnityWebRequest www = UnityWebRequest.Put(validationEndpoint + appID, postData))
            {
                www.SetRequestHeader("content-type", "application/json");
                yield return www.SendWebRequest();

                //raw JSON response
                try
                {
                    rawResponse = JSON.Parse(www.downloadHandler.text);
                    success = www.error == null && rawResponse != null && string.IsNullOrEmpty(rawResponse["error"]) && rawResponse.HasKey("data");
                }
                catch
                {
                    //there might be an configuration issue since the response was not valid JSON, do not finish transaction and print error message directly
                    if (IAPManager.isDebug) Debug.Log("Server Validation: Product purchase '" + product.definition.storeSpecificId + "' failed.\n" + www.downloadHandler.text);
                    IAPManager.OnPurchaseFailed("Your Purchase: " + product.metadata.localizedTitle + "\n\nYour transaction failed validation.");
                    yield break;
                }

                if(success)
                {
                    if (IAPManager.isDebug) Debug.Log("Server Validation: Product purchase '" + product.definition.id + "' success.");
                    IAPManager.controller.ConfirmPendingPurchase(product);
                    
                    string productId = product.definition.id;
                    PurchaseResponse thisPurchase = JsonUtility.FromJson<PurchaseResponse>(rawResponse["data"].ToString());

                    if (inventory.ContainsKey(productId)) inventory[productId] = thisPurchase; //already exist, replace
                    else inventory.Add(productId, thisPurchase); //add new to inventory

                    //check purchasing state in raw data
                    if (IsPurchased(productId)) IAPManager.GetInstance().CompletePurchase(productId);
                    else RemovePurchase(productId);
                }
                else
                {
                    int errorCode = rawResponse.HasKey("error") ? rawResponse["code"].AsInt : -1;
                    //do not complete pending purchases but still leave them open for processing again later
                    if (errorCode == 10130) yield break;

                    if (IAPManager.isDebug) Debug.Log("Server Validation: Product purchase '" + product.definition.storeSpecificId + "' failed. Code:" + errorCode + ", " + rawResponse["error"].Value);
                    IAPManager.controller.ConfirmPendingPurchase(product);
                    IAPManager.OnPurchaseFailed("Your Purchase: " + product.metadata.localizedTitle + "\n\nYour transaction failed validation.");
                    RemovePurchase(product.definition.id);
                }
            }

            purchaseCallback?.Invoke(product.definition.id, rawResponse);
        }


        IEnumerator RequestRestoreRoutine()
        {
            foreach(Product product in IAPManager.controller.products.all)
            {
                if (!product.hasReceipt || product.definition.type == ProductType.Consumable || inventory.ContainsKey(product.definition.id))
                    continue;

                if(!DBManager.IsPurchased(product.definition.id))
                {
                    Validate(product);
                    yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(2f, 5f));
                }
            }
        }


        /// <summary>
        /// Return current user inventory stored in memory.
        /// </summary>
        public Dictionary<string, PurchaseResponse> GetInventory()
        {
            return inventory;
        }


        bool IsPurchased(string productId)
        {
            if (inventoryRequestType == InventoryRequestType.Disabled)
            {
                return IAPManager.controller.products.WithID(productId).hasReceipt;
            }

            int[] purchaseStates = new int[] { 0, 1, 4 };
            if (inventory.ContainsKey(productId) && Array.Exists(purchaseStates, x => x == inventory[productId].status))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns whether getting inventory is currently disabled, limited or not possible.
        /// </summary>
        public bool CanRequestInventory()
        {
            //GetInventory request is already active. This call was cancelled
            if (inventoryRequestActive)
            {
                return false;
            }

            switch (inventoryRequestType)
            {
                //GetInventory call is disabled. If your plan supports User Inventory, select a different Inventory Request Type
                case InventoryRequestType.Disabled:
                    return false;

                //GetInventory call was cancelled because it has already been requested before
                case InventoryRequestType.Once:
                    if (lastInventoryTime > 0)
                    {
                        return false;
                    }
                    break;

                //GetInventory call was cancelled to prevent excessive bandwidth consumption and API limits
                case InventoryRequestType.Delay:
                    if (lastInventoryTime > 0 && Time.realtimeSinceStartup - lastInventoryTime < inventoryDelay)
                    {
                        return false;
                    }
                    break;
            }

            //All checks passed, but a user identifier has not been set
            if (string.IsNullOrEmpty(userID))
            {
                return false;
            }

            return true;
        }


        void RemovePurchase(string productId)
        {
            //check whether the product was set to purchased locally
            if (IAPManager.GetIAPProduct(productId).type == ProductType.Consumable || !DBManager.IsPurchased(productId))
                return;

            ShopItem2D item = null;
            if (IAPManager.GetInstance()) item = IAPManager.GetShopItem(productId);
            if (item) item.Purchased(false);
            DBManager.ConsumePurchase(productId);
        }


        void SetPurchaseHistory()
        {
            if (DBManager.IsPlayerData(lastInventoryTimestampKey) && inventory.Count == 0)
            {
                DBManager.ConsumePlayerData(lastInventoryTimestampKey);
                return;
            }

            if (!DBManager.IsPlayerData(lastInventoryTimestampKey) && inventory.Count > 0)
            {
                DBManager.SetPlayerData(lastInventoryTimestampKey, new JSONData(DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()));
            }
        }


        bool HasPurchaseHistory()
        {
            if (!DBManager.IsPlayerData(lastInventoryTimestampKey))
                return false;

            long lastTimestamp = long.Parse(DBManager.GetPlayerData(lastInventoryTimestampKey).Value);
            long timestampNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (timestampNow - lastTimestamp < 2628000) //2628000 seconds = 1 month
            {
                return true;
            }

            DBManager.ConsumePlayerData(lastInventoryTimestampKey);
            return false;
        }


        bool HasActiveReceipt()
        {
            bool hasReceipt = false;

            #if UNITY_ANDROID
                foreach (Product product in IAPManager.controller.products.all)
                {
                    if (product.definition.type != ProductType.Consumable && product.hasReceipt)
                    {
                        hasReceipt = true;
                        break;
                    }
                }
            #elif UNITY_IOS
                IAppleConfiguration appleConfig = IAPManager.builder.Configure<IAppleConfiguration>();
                if (string.IsNullOrEmpty(appleConfig.appReceipt)) return false;
                byte[] appReceipt = Convert.FromBase64String(appleConfig.appReceipt);
                AppleReceipt appleReceipt = new AppleValidator(AppleTangle.Data()).Validate(appReceipt);

                if(appleReceipt.inAppPurchaseReceipts.Length > 0)
                {
                    foreach(AppleInAppPurchaseReceipt receipt in appleReceipt.inAppPurchaseReceipts)
                    {
                        switch(receipt.productType)
                        {
                            case 0: //NonConsumable
                            case 2: //Non-Renewing Subscription
                                if (receipt.cancellationDate == DateTime.MinValue) hasReceipt = true;
                                break;
                            case 3: //Auto-Renewing Subscription
                                if (receipt.subscriptionExpirationDate != DateTime.MinValue) hasReceipt = true;
                                break;
                        }

                        if (hasReceipt == true) break;
                    }
                }
            #endif

            return hasReceipt;
        }


        PurchaseState LocalValidation(Product product)
        {
            try
            {
                IPurchaseReceipt[] result = localValidator.Validate(product.receipt);

                foreach (IPurchaseReceipt receipt in result)
                {
                    if (receipt is GooglePlayReceipt googleReceipt)
                    {
                        if ((int)googleReceipt.purchaseState == 2 || (int)googleReceipt.purchaseState == 4)
                        {
                            return PurchaseState.Pending;
                        }
                    }
                }

                return PurchaseState.Purchased;
            }
            //If the purchase is deemed invalid, the validator throws an exception
            catch (IAPSecurityException)
            {
                return PurchaseState.Failed;
            }
        }


        string GetType(ProductType type)
        {
            switch (type)
            {
                case ProductType.Consumable:
                case ProductType.Subscription:
                    return type.ToString();

                default:
                    return "Non-Consumable";
            }
        }


        public bool IsLocalValidationSupported()
        {
            AppStore currentAppStore = StandardPurchasingModule.Instance().appStore;

            //The CrossPlatform validator only supports the GooglePlayStore and Apple's App Stores.
            if (currentAppStore == AppStore.GooglePlay ||
               currentAppStore == AppStore.AppleAppStore || currentAppStore == AppStore.MacAppStore)
                return true;

            return false;
        }


        public bool IsServerValidationSupported()
        {
            AppStore currentAppStore = StandardPurchasingModule.Instance().appStore;

            //The server validator only supports the GooglePlayStore and Apple's App Stores.
            if (currentAppStore == AppStore.GooglePlay ||
               currentAppStore == AppStore.AppleAppStore || currentAppStore == AppStore.MacAppStore)
                return true;

            return false;
        }
        #endif
    }
    #endif


    /// <summary>
    /// Available options for fetching User Inventory.
    /// </summary>
    public enum InventoryRequestType
    {
        Disabled,
        Manual,
        Once,
        Delay
    }


    enum PurchaseState
    {
        Purchased,
        Pending,
        Failed
    }


    [System.Serializable]
    struct ReceiptRequest
    {
        public string store;
        public string bid;
        public string pid;
        public string type;
        public string user;
        public string receipt;
    }


    [System.Serializable]
    public struct PurchaseResponse
    {
        public int status;
        public string type;
        public int expiresDate;
        public bool autoRenew;
        public bool billingRetry;
        public string productId;
        public bool sandbox;
    }
}
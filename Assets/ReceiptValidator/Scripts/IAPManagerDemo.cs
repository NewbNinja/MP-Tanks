using SimpleJSON;
using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace FLOBUK.ReceiptValidator.Demo
{
    /// <summary>
    /// Unity IAP Demo Implementation.
    /// </summary>
    public class IAPManagerDemo : MonoBehaviour, IStoreListener
    {
        private static IAPManagerDemo instance;
        public static event Action<Color, string> debugCallback;
        public static event Action<bool, JSONNode> purchaseCallback;

        //product identifiers for App Stores
        [Header("Product IDs")]
        public string consumableProductId = "coins";
        public string nonconsumableProductId = "no_ads";
        public string subscriptionProductId = "abo_monthly";

        //Unity IAP references
        public IStoreController controller;
        IExtensionProvider extensions;
        ConfigurationBuilder builder;


        //return the instance of this script.
        public static IAPManagerDemo GetInstance()
        {
            return instance;
        }


        //create a persistent script instance
        void Awake()
        {
            if (instance)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(this);

            instance = this;
        }


        //initialize Unity IAP with demo products
        void Start()
        {
            builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            builder.AddProduct(consumableProductId, ProductType.Consumable);
            builder.AddProduct(nonconsumableProductId, ProductType.NonConsumable);
            builder.AddProduct(subscriptionProductId, ProductType.Subscription);

            UnityPurchasing.Initialize(this, builder);
        }


        //fired when Unity IAP initialization completes successfully
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            DebugLogText(Color.green, "In-App Purchasing successfully initialized");
            this.controller = controller;
            this.extensions = extensions;

            //initialize ReceiptValidator
            ReceiptValidator.Instance.Initialize(controller, builder);
            ReceiptValidator.purchaseCallback += OnPurchaseResult;
            //if you are making use of user inventory
            ReceiptValidator.Instance.RequestInventory();
        }


        //fired when Unity IAP receives a purchase which is then ready for local and server-side validation
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            Product product = args.purchasedProduct;

            //do validation, the magic happens here!
            PurchaseState state = ReceiptValidator.Instance.RequestPurchase(product);
            //handle what happens with the product next
            switch (state)
            {
                case PurchaseState.Purchased:
                    //nothing to do here: with the transaction finished at this point it means that either
                    //1) local validation passed but server validation is not supported, or
                    //2) validation is not supported at all, e.g. when running on a non-supported store
                    break;

                //transaction is pending or about to be validated on the server
                //it is important to return pending to leave the transaction open for the ReceiptValidator
                //the ReceiptValidator will fire its purchaseCallback when done processing
                case PurchaseState.Pending: 
                    DebugLogText(Color.white, "Product purchase '" + product.definition.storeSpecificId + "' is pending.");
                    return PurchaseProcessingResult.Pending;

                //transaction invalid or failed locally. Complete transaction to not validate again
                case PurchaseState.Failed: 
                    DebugLogText(Color.red, "Product purchase '" + product.definition.storeSpecificId + "' deemed as invalid.");
                    break;
            }


            //with the transaction finished, just call our purchase handler. You would do your own purchase handling here as usual.
            //we just pass in the same variables as received from the ReceiptValidator, which is what our demo UI expects.
            JSONObject resultData = new JSONObject();
            resultData["data"]["productId"] = product.definition.id;
            OnPurchaseResult(state == PurchaseState.Purchased, resultData);

            return PurchaseProcessingResult.Complete;
        }


        //request re-validation of local receipts on the server, in case they do not match.
        //do not call this on every app launch! This should be manually triggered by the user.
        public void RestoreTransactions()
        {
            if (controller == null)
            {
                DebugLogText(Color.yellow, "Unity IAP is not initialized yet.");
                return;
            }

            DebugLogText(Color.white, "Trying to restore transactions...");

            #if UNITY_IOS
			    extensions.GetExtension<IAppleExtensions>().RestoreTransactions(result => { DebugLogText(Color.white, "RestoreTransactions result: " + result); });
            #else
                ReceiptValidator.Instance.RequestRestore();
            #endif
        }


        //fired when Unity IAP failed to initialize.
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            DebugLogText(Color.red, $"In-App Purchasing initialize failed: {error}");
        }


        //fired when Unity IAP failed to process a purchase.
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            DebugLogText(Color.red, $"Purchase failed - Product: '{product.definition.id}', PurchaseFailureReason: {failureReason}");
            purchaseCallback(false, null);
        }


        //this either comes from the ProcessPurchase handler when a transaction finished,
        //but also from the Receipt Validator when a server validation request completes.
        //we are using the same parameters to keep them consistent.
        public void OnPurchaseResult(bool success, JSONNode data)
        {
            //we just fire our IAPManager callback to update the UI
            purchaseCallback(success, data);
        }


        //callback for UI display purposes
        void DebugLogText(Color color, string text)
        {
            debugCallback?.Invoke(color, text);
        }
    }
}
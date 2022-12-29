/*  This file is part of the "Tanks Multiplayer" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

#if UNITY_IAP
using Unity.Services.Core;
using UnityEngine.Purchasing;
#endif
#if RECEIPT_VALIDATION
using FLOBUK.ReceiptValidator;
#endif

using System;
using UnityEngine;

namespace TanksMP
{
    /// <summary>
    /// Manager handling the full in-app purchase workflow,
    /// granting purchases and catching errors using Unity IAP.
    /// </summary>
    #if UNITY_IAP
    public class UnityIAPManager : MonoBehaviour, IStoreListener
    #else
    public class UnityIAPManager : MonoBehaviour
    #endif
    {
        #pragma warning disable 0067
        /// <summary>
        /// Fired on failed purchases to deliver its product identifier.
        /// </summary>
        public static event Action<string> purchaseFailedEvent;
        #pragma warning restore 0067

        #if UNITY_IAP
        //disable platform specific warnings, because Unity throws them
        //for unused variables however they are used in this context
        #pragma warning disable 0414
        private static ConfigurationBuilder builder;
        private static IStoreController controller;
        private static IExtensionProvider extensions;
        #pragma warning restore 0414



        void Start()
        {
            //construct IAP purchasing instance and add the Google Play public key to it
            builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            //iterate over all IAPProducts found in the scene and add their id to be looked up by Unity IAP
            IAPProduct[] products = Resources.FindObjectsOfTypeAll<IAPProduct>();
            foreach(IAPProduct product in products)
            {
				if(product.buyable)
                	builder.AddProduct(product.id, product.type);
            }

            Initialize();
        }


        /// <summary>
        /// Initialize core services and Unity IAP.
        /// </summary>
        public async void Initialize()
        {
            try
            {
                //Unity Gaming Services are required first
                await UnityServices.InitializeAsync();

                //now we're ready to initialize Unity IAP
                UnityPurchasing.Initialize(this, builder);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                OnInitializeFailed(InitializationFailureReason.PurchasingUnavailable);
            }
        }


        /// <summary>
        /// Called when Unity IAP is ready to make purchases, delivering the store controller
        /// (contains all online products) and platform specific extension
        /// </summary>
        public void OnInitialized (IStoreController ctrl, IExtensionProvider ext)
        {
            //cache references
            controller = ctrl;
            extensions = ext;

            #if RECEIPT_VALIDATION
                ReceiptValidator.Instance.Initialize(controller, builder);
                ReceiptValidator.purchaseCallback += OnPurchaseResult;
            #endif
        }


        /// <summary>
        /// Called when the user presses the 'Buy' button on an IAPProduct.
        /// </summary>
        public static void PurchaseProduct(string productId)
        {
            if(controller != null)
               controller.InitiatePurchase(productId);
        }


        /// <summary>
        /// Called when Unity IAP encounters an unrecoverable initialization error.
        /// </summary>
        public void OnInitializeFailed (InitializationFailureReason error)
        {
			Debug.Log(error);
        }


        /// <summary>
        /// Called when a purchase completes after being bought.
        /// </summary>
        public PurchaseProcessingResult ProcessPurchase (PurchaseEventArgs e)
        {
            Product product = e.purchasedProduct;

            #if RECEIPT_VALIDATION
                PurchaseState state = ReceiptValidator.Instance.RequestPurchase(product);
                //handle what happens with the product next
                switch (state)
                {
                    case PurchaseState.Pending:
                        return PurchaseProcessingResult.Pending;
                    case PurchaseState.Failed:
                        product = null;
                        break;
                }                
            #endif

            //with the transaction finished, just call our purchase method
            if(product != null)
            { 
                OnPurchaseSuccess(product);
            }

            //return that we are done with processing the transaction
            return PurchaseProcessingResult.Complete;
        }


        #if RECEIPT_VALIDATION
        void OnPurchaseResult(bool success, SimpleJSON.JSONNode data)
        {
            if (!success) return;

            Product product = controller.products.WithID(data["data"]["productId"]);
            OnPurchaseSuccess(product);
        }
        #endif


        void OnPurchaseSuccess(Product purchasedProduct)
        {
            //get all IAPProduct references in the scene, then loop over them
            IAPProduct[] products = FindObjectsOfType(typeof(IAPProduct)) as IAPProduct[];
            foreach (IAPProduct product in products)
            {
                //we have found the IAPProduct instance we have bought right now
                if (product.id == purchasedProduct.definition.id)
                {
                    //set the product to purchased
                    //if it is selectable, show its select button
                    product.Purchased();
                    if (product.selectButton)
                        product.IsSelected(true);
                    break;
                }
            }

            //save the encrypted identifier of this product on the device to keep it as purchased
            PlayerPrefs.SetString(Encryptor.Encrypt(purchasedProduct.definition.id), "");
            PlayerPrefs.Save();
        }


        /// <summary>
        /// Called when a purchase fails, providing the product and reason.
        /// </summary>
        public void OnPurchaseFailed (Product p, PurchaseFailureReason r)
        {
            if(purchaseFailedEvent != null)
                purchaseFailedEvent(r.ToString());
        }


        /// <summary>
        /// Method for restoring transactions (prompts for password on iOS).
        /// </summary>
        public static void RestoreTransactions()
        {
            foreach (Product p in controller.products.all)
            {	
                if(!PlayerPrefs.HasKey(Encryptor.Encrypt(p.definition.id)))
                    PlayerPrefs.DeleteKey(Encryptor.Encrypt(p.definition.id));
            }

            #if UNITY_IOS
            if(extensions != null)
			    extensions.GetExtension<IAppleExtensions>().RestoreTransactions(null);
            #elif RECEIPT_VALIDATION
                ReceiptValidator.Instance.RequestRestore();
            #endif
        }
        #endif
    }
}
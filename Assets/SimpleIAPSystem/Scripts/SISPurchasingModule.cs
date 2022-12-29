/*  This file is part of the "Simple IAP System" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System.Collections.Generic;
using UnityEngine;

#if SIS_IAP
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace SIS
{
    /// <summary>
    /// Custom Unity IAP purchasing module for overwriting default store subsystems.
    /// </summary>
    public class SISPurchasingModule : IPurchasingModule
    {
        /// <summary>
        /// The currently selected App Store.
        /// </summary>
        public string appStore = "";

        /// <summary>
        /// Custom Store implementations that have been enabled.
        /// </summary>
        public Dictionary<string, IStore> customStores = new Dictionary<string, IStore>();


        public SISPurchasingModule(IAPManager callback)
        {
            if(callback.asset.customStoreConfig.PayPal.enabled)
                customStores.Add(IAPPlatform.PayPal.ToString(), new PayPalStore(callback));
        }


        public void Configure(IPurchasingBinder binder)
		{
            //Native
            #if STEAM_IAP
                appStore = IAPPlatform.SteamStore.ToString();
                binder.RegisterStore(appStore, new SteamStore());
            #endif

            //VR
            #if OCULUS_IAP
                appStore = IAPPlatform.OculusStore.ToString();
                binder.RegisterStore(appStore, new OculusStore());
            #endif

            //PlayFab
            if(PlayFabManager.GetInstance() != null)
            {
                #if !PLAYFAB_PAYPAL && !PLAYFAB_STEAM
                    //in case of default store on Google Play/iOS
                    //try to initialize and retrieve products once
                    new PlayFabStore();
                #endif

                #if PLAYFAB_PAYPAL
                    appStore = IAPPlatform.PayPal.ToString();
                    binder.RegisterStore(appStore, new PlayFabPayPalStore());
                #endif

                #if PLAYFAB_STEAM
                    appStore = IAPPlatform.SteamStore.ToString();
                    binder.RegisterStore(appStore, new PlayFabSteamStore());
                #endif
            }
           
            //Extensions
            binder.RegisterExtension(new SISPurchasingExtension(this));
        }
    }


    public class SISPurchasingExtension : IStoreExtension
    {
        private SISPurchasingModule module;

        public SISPurchasingExtension(SISPurchasingModule module)
        {
            this.module = module;
        }

        public void Purchase(ProductDefinition definition, BillingProvider provider)
        {
            if(!module.customStores.ContainsKey(provider.ToString()))
            {
                Debug.LogWarning("Custom Store " + provider.ToString() + " is not enabled!");
                return;
            }

            module.customStores[provider.ToString()].Purchase(definition, string.Empty);
        }
    }
}
#endif
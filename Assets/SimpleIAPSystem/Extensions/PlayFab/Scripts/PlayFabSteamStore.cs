/*  This file is part of the "Simple IAP System" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using UnityEngine;

using UnityEngine.Purchasing.Extension;

using PlayFab;
using PlayFab.ClientModels;

#if PLAYFAB_STEAM
using Steamworks;
#endif

namespace SIS
{
    #if !PLAYFAB_STEAM
    public class PlayFabSteamStore : PlayFabStore
    { }
    #endif

    #if PLAYFAB_STEAM
    /// <summary>
    /// Represents the public interface of the underlying store system for the Steamworks API.
    /// </summary>
    public class PlayFabSteamStore : PlayFabStore
    {
        #pragma warning disable 0414
        protected Callback<MicroTxnAuthorizationResponse_t> microTxnAuthorizationResponse;
        #pragma warning restore 0414


        /// <summary>
        /// Overriding the initialization with setting the correct store.
        /// </summary>
        public override void Initialize(IStoreCallback callback)
        {
            storeId = "Steam";
            this.callback = callback;

            if (!SteamManager.Initialized)
            {
                return;
            }

            microTxnAuthorizationResponse = Callback<MicroTxnAuthorizationResponse_t>.Create(OnMicroTxnAuthorizationResponse);
        }


        /// <summary>
        /// Overriding the payment request for opening the Steam overlay. Nothing to do here, since PlayFab handles this.
        /// </summary>
		public override void OnPurchaseResult(PayForPurchaseResult result)
		{
            /*
            Debug.LogError("Steam Purchase Status: " + result.Status + ", Currency: " + result.PurchaseCurrency +
                ", Price: " + result.PurchasePrice + ", ProviderData: " + result.ProviderData +
                ", PageURL: " + result.PurchaseConfirmationPageURL);
            */
		}


        /// <summary>
        /// Manually triggering purchase confirmation after a Steam payment has been made in the overlay.
        /// This is so that the transaction gets finished and Steam actually substracts funds.
        /// </summary>
        private void OnMicroTxnAuthorizationResponse(MicroTxnAuthorizationResponse_t pCallback)
        {
            //Debug.LogError("Steam MicroTxn Response: " + pCallback.m_unAppID + ", " + pCallback.m_ulOrderID + ", " + pCallback.m_bAuthorized);

			ConfirmPurchaseRequest request = new ConfirmPurchaseRequest()
			{
				OrderId = orderId
			};

			PlayFabClientAPI.ConfirmPurchase(request, OnPurchaseSucceeded, OnPurchaseFailed);
        }
    }
    #endif
}
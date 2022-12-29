/*  This file is part of the "Simple IAP System" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SIS
{
    /// <summary>
    /// Displaying purchase confirmation for finishing transactions required on certain stores, 
    /// e.g. when using PayPal store. Confirming payments is a manual action so this
    /// script should be somewhere in your shop UI. Otherwise user rewards could be lost.
    /// </summary>
    public class UIWindowConfirm : MonoBehaviour
    {
        /// <summary>
        /// Button to trigger transaction confirmation on the store.
        /// </summary>
        public GameObject okButton;

        /// <summary>
        /// Button to close the window (eventually without confirming transactions).
        /// </summary>
        public GameObject cancelButton;

        /// <summary>
        /// Loading indicator for the user to see that something is going on.
        /// </summary>
        public Image loadingImage;


        //rotate the loading indicator for visual representation
        void Update()
        {
            loadingImage.rectTransform.Rotate(-Vector3.forward * 100 * Time.deltaTime);
        }


        #if SIS_IAP
        //start displaying the UI buttons after some time
        void OnEnable()
        {
            StartCoroutine(UpdateStatus());
        }


        //hide UI buttons for some time to actually give the user the chance for payment
        private IEnumerator UpdateStatus()
        {
            bool isActive = false;
            if (PayPalStore.instance != null) isActive = true;
            #if PLAYFAB_PAYPAL
            if (PlayFabPayPalStore.instance != null) isActive = true;
            #endif

            //no PayPalStore found, cancel
            if(!isActive)
            {
                cancelButton.SetActive(true);
                yield break;
            }
            
            yield return new WaitForSeconds(10);
            cancelButton.SetActive(true);

            //display auto-refresh mechanic, require a manual confirmation of the order
            okButton.SetActive(true);
        }


        /// <summary>
        /// Triggers transaction confirmation on the store.
        /// Usually assigned to a UI button in-game.
        /// </summary>
        public void Confirm()
        {
            if (PayPalStore.instance != null) PayPalStore.instance.ConfirmPurchase();
            #if PLAYFAB_PAYPAL
            if (PlayFabStore.instance != null && PlayFabStore.instance is PlayFabPayPalStore)
               (PlayFabStore.instance as PlayFabPayPalStore).ConfirmPurchase();
            #endif

            StartCoroutine(Delay());
        }
        #endif


        //delay further confirm request within the timeout frame
        private IEnumerator Delay()
        {
            okButton.SetActive(false);
            yield return new WaitForSeconds(10);
            okButton.SetActive(true);
        }


        //reset window to original state
        void OnDisable()
        {
            StopAllCoroutines();
            okButton.SetActive(false);
            cancelButton.SetActive(false);
        }
    }
}

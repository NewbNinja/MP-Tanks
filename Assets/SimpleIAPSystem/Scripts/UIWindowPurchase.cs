/*  This file is part of the "Simple IAP System" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using UnityEngine;
using UnityEngine.UI;

namespace SIS
{
    /// <summary>
    /// Purchase window asking the user to confirm their virtual purchase or spending virtual currency.
    /// Optional. Note that real money purchases do not need this, as they invoke their own native popup.
    /// </summary>
    public class UIWindowPurchase : MonoBehaviour
    {
        /// <summary>
        /// The label for the product title to be purchased.
        /// </summary>
        public Text label;

        /// <summary>
        /// The product's icon visualization.
        /// </summary>
        public Image icon;

        //ID of the product. Cached for later use when confirming the window.
        private string productID;


        /// <summary>
        /// Initialize window.
        /// </summary>
        public void Set(string productID)
        {
            IAPProduct product = IAPManager.GetIAPProduct(productID);
            if (product == null) return;

            this.productID = productID;
            label.text = product.title;
            icon.sprite = product.icon;

            gameObject.SetActive(true);
        }


        /// <summary>
        /// User accepted, try to buy product.
        /// </summary>
        public void Purchase()
        {
            IAPManager.Purchase(productID);

            gameObject.SetActive(false);
        }


        //reset window to original state
        void OnDisable()
        {
            productID = "";
            label.text = "";
            icon.sprite = null;
        }
    }
}

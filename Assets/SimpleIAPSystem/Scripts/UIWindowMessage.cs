/*  This file is part of the "Simple IAP System" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using UnityEngine;
using UnityEngine.UI;

namespace SIS
{
    /// <summary>
    /// Message window for showing feedback on purchase events to the user.
    /// It is highly recommended to implement this so that users get feedback on the purchase workflow.
    /// </summary>
    public class UIWindowMessage : MonoBehaviour
    {
        /// <summary>
        /// The text label for the message displayed to the user.
        /// </summary>
        public Text label;


        /// <summary>
        /// Initialize window.
        /// </summary>
        public void Set(string text)
        {
            label.text = text;
            gameObject.SetActive(true);
        }


        //reset window to original state
        void OnDisable()
        {
            label.text = "";
        }
    }
}

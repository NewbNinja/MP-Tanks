using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;

namespace FLOBUK.ReceiptValidator.Demo
{
    /// <summary>
    /// Displays Server and Validation responses.
    /// </summary>
    public class UIDemo : MonoBehaviour
    {
        //display of purchased state or amount
        [Header("UI Texts")]
        public Text ConsumableText;
        public Text NonConsumableText;
        public Text SubscriptionText;

        //display of currently active validation tasks
        public Text ProcessingPurchasesCountText;
        //display of chosen user ID / name
        public Text UserText;
        //display of system and server messages
        public Text ResponseText;
        //notice when running on non-mobile platforms
        public GameObject InfoObject;

        string userNameBase = "user";
        int userIndex = 0;
        int consumableCount;
        int processingPurchasesCount;

        private IAPManagerDemo instance;


        void Start()
        {
            #if !UNITY_EDITOR
                InfoObject.SetActive(false);
            #endif

            //get instance
            instance = IAPManagerDemo.GetInstance();
            if (!instance) return;

            //subscribe to callbacks
            ReceiptValidator.inventoryCallback += InventoryRetrieved;
            IAPManagerDemo.purchaseCallback += PurchaseResult;
            IAPManagerDemo.debugCallback += PrintMessage;

            UpdateUI();
        }


        //ReceiptValidator.inventoryCallback
        void InventoryRetrieved()
        {
            PrintMessage(Color.green, "Inventory retrieved.");

            Dictionary<string, PurchaseResponse> inventory = ReceiptValidator.Instance.GetInventory();
            foreach (string productID in inventory.Keys)
                PrintMessage(Color.white, productID + ": " + inventory[productID].ToString());

            UpdateUI();
        }


        //buy buttons for different product types
        public void BuyConsumable() { Buy(instance.consumableProductId); }
        public void BuyNonconsumable() { Buy(instance.nonconsumableProductId); }
        public void BuySubscription() { Buy(instance.subscriptionProductId); }


        //buy method triggering Unity IAP
        void Buy(string productId)
        {
            processingPurchasesCount++;
            UpdateUI();

            instance.controller.InitiatePurchase(productId);
        }


        //IAPManagerDemo.purchaseCallback
        //(incorporates ReceiptValidator.purchaseCallback)
        void PurchaseResult(bool success, JSONNode result)
        {
            processingPurchasesCount--;

            switch(success)
            {
                case true:
                    PrintMessage(Color.green, "Purchase validation success!");

                    //on consumble products add them to the players inventory locally
                    if (result["data"]["productId"] == instance.consumableProductId)
                    {
                        consumableCount++;
                    }
                    break;

                case false:
                    PrintMessage(Color.red, "Purchase validation failed.");
                    break;
            }

            if(result != null)
                PrintMessage(Color.white, "Raw: " + result.ToString());

            UpdateUI();
        }


        //message display
        void PrintMessage(Color color, string text)
        {
            ResponseText.text = "\n\n" + "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + text + "</color>" + ResponseText.text;
        }


        //switch between users for debugging purposes
        public void ChangeUser()
        {
            if (userIndex == 0 && ReceiptValidator.Instance.userID != userNameBase + "0")
            {
                PrintMessage(Color.yellow, "UserName was set manually and will not be overridden.");
                UpdateUI();
                return;
            }

            userIndex++;

            if (userIndex == 6)
                userIndex = 0;

            ReceiptValidator.Instance.userID = userNameBase + userIndex;
            UpdateUI();
        }


        //try to get inventory
        public void GetInventory()
        {
            if(!ReceiptValidator.Instance.CanRequestInventory())
            {
                PrintMessage(Color.red, "Inventory call is not possible. If you are on a paid plan, check your selected Inventory Request Type.");
                return;
            }

            ReceiptValidator.Instance.RequestInventory();
        }


        //update graphical display of text contents with current states
        void UpdateUI()
        {
            UserText.text = ReceiptValidator.Instance.userID;

            ConsumableText.text = $"Count: {consumableCount}";
            NonConsumableText.text = ReceiptValidator.Instance.IsPurchased(instance.nonconsumableProductId) ? "Purchased" : "Not Purchased";
            SubscriptionText.text = ReceiptValidator.Instance.IsPurchased(instance.subscriptionProductId) ? "Purchased" : "Not Purchased";

            ProcessingPurchasesCountText.text = "";
            for (int i = 0; i < processingPurchasesCount; i++)
            {
                ProcessingPurchasesCountText.text += "Purchase Processing...\n";
            }
        }
    }
}

using CBS.Core;
using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using CBS.Utils;

namespace CBS.UI
{
    public class ItemUI : MonoBehaviour, IScrollableItem<CBSBaseItem>
    {
        [SerializeField]
        private Text Title;
        [SerializeField]
        private Text Count;
        [SerializeField]
        private Image Icon;
        [SerializeField]
        private Transform CurrencyRoot;

        private CBSItem Item { get; set; }
        private ItemsIcons ItemIcons { get; set; }
        private CommonPrefabs Prefabs { get; set; }
        private ICBSItems CBSItems { get; set; }

        private List<GameObject> CurrencyPool { get; set; } = new List<GameObject>();

        private void Awake()
        {
            ItemIcons = CBSScriptable.Get<ItemsIcons>();
            Prefabs = CBSScriptable.Get<CommonPrefabs>();
            CBSItems = CBSModule.Get<CBSItems>();
        }

        public void Display(CBSBaseItem data)
        {
            Item = data as CBSItem;
            // draw icons
            Icon.sprite = ItemIcons.GetSprite(Item.ID);
            // draw name
            Title.text = Item.DisplayName;
            // display count
            bool isUsable = Item.IsConsumable;
            string countText = isUsable ? Item.UsageCount.ToString() : string.Empty;
            Count.text = countText;
            // display prices
            foreach (var obj in CurrencyPool)
                obj.SetActive(false);

            var currencies = Item.Prices;
            for (int i = 0;i<currencies.Count;i++)
            {
                var price = currencies.ElementAt(i);
                string key = price.Key;
                int val = (int)price.Value;

                if (i >= CurrencyPool.Count)
                {
                    var pricePrefab = Prefabs.PurchaseButton;
                    var priceUI = Instantiate(pricePrefab, CurrencyRoot);
                    CurrencyPool.Add(priceUI);
                    priceUI.GetComponent<PurchaseButton>().Display(key, val, Buy);
                }
                else
                {
                    CurrencyPool[i].SetActive(true);
                    CurrencyPool[i].GetComponent<PurchaseButton>().Display(key, val, Buy);
                }
            }
        }

        // buttons events
        private void Buy(string key, int value)
        {
            string itemID = Item.ID;
            if (string.IsNullOrEmpty(itemID))
                return;
            CBSItems.PurchaseItem(itemID, key, value, OnItemPurchased);
        }

        private void OnItemPurchased(CBSPurchaseItemResult result)
        {
            if (result.IsSuccess)
            {
                new PopupViewer().ShowSimplePopup(new PopupRequest { 
                    Title = ItemTXTHandler.PurchaseTitle,
                    Body = ItemTXTHandler.PurchaseBody
                });
            }
            else
            {
                Debug.Log("Failed to purchase item. "+result.Error.Message);
                new PopupViewer().ShowFabError(result.Error);
            }
        }
    }
}

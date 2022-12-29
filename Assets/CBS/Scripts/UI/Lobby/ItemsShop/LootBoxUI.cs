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
    public class LootBoxUI : MonoBehaviour, IScrollableItem<CBSBaseItem>
    {
        [SerializeField]
        private Text Title;
        [SerializeField]
        private Text Description;
        [SerializeField]
        private Image Icon;
        [SerializeField]
        private Transform CurrencyRoot;
        [SerializeField]
        private Transform BundleRoot;

        private CBSLootbox LootBox { get; set; }
        private ItemsIcons ItemIcons { get; set; }
        private CommonPrefabs Prefabs { get; set; }
        private ICBSItems CBSItems { get; set; }

        private List<GameObject> CurrencyPool { get; set; } = new List<GameObject>();
        private List<GameObject> LootPool { get; set; } = new List<GameObject>();

        private void Awake()
        {
            ItemIcons = CBSScriptable.Get<ItemsIcons>();
            Prefabs = CBSScriptable.Get<CommonPrefabs>();
            CBSItems = CBSModule.Get<CBSItems>();
        }

        public void Display(CBSBaseItem data)
        {
            LootBox = data as CBSLootbox;
            // draw icons
            Icon.sprite = ItemIcons.GetSprite(LootBox.ID);
            // draw name
            Title.text = LootBox.DisplayName;
            // draw description
            Description.text = LootBox.Description;
            // display prices
            foreach (var obj in CurrencyPool)
                obj.SetActive(false);

            var currencies = LootBox.Prices;
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
            // draw loot items
            foreach (var obj in LootPool)
                obj.SetActive(false);

            var loot = LootBox.RandomItemsIDs;
            var lootCurrency = LootBox.PackCurrecnies;

            for (int i = 0; i < loot.Count; i++)
            {
                var itemDI = loot.ElementAt(i);

                if (i >= LootPool.Count)
                {
                    var iconPrefab = Prefabs.SimpleIcon;
                    var bundleUI = Instantiate(iconPrefab, BundleRoot);
                    LootPool.Add(bundleUI);
                    bundleUI.GetComponent<SimpleIcon>().DrawItem(itemDI);
                }
                else
                {
                    LootPool[i].SetActive(true);
                    LootPool[i].GetComponent<SimpleIcon>().DrawItem(itemDI);
                }
            }

            for (int i = 0; i < lootCurrency.Count; i++)
            {
                var co = lootCurrency.ElementAt(i);
                string currencyID = co.Key;
                int value = (int)co.Value;

                if ((i + loot.Count) >= LootPool.Count)
                {
                    var iconPrefab = Prefabs.SimpleIcon;
                    var bundleUI = Instantiate(iconPrefab, BundleRoot);
                    LootPool.Add(bundleUI);
                    bundleUI.GetComponent<SimpleIcon>().DrawCurrency(currencyID);
                    bundleUI.GetComponent<SimpleIcon>().DrawValue(value.ToString());
                }
                else
                {
                    LootPool[i + loot.Count].SetActive(true);
                    LootPool[i + loot.Count].GetComponent<SimpleIcon>().DrawCurrency(currencyID);
                    LootPool[i + loot.Count].GetComponent<SimpleIcon>().DrawValue(value.ToString());
                }
            }
        }

        // buttons events
        public void Buy(string key, int value)
        {
            string lootBoxID = LootBox.ID;
            if (string.IsNullOrEmpty(lootBoxID))
                return;
            CBSItems.PurchaseLootbox(lootBoxID, key, value, OnLootBoxPurchased);
        }

        private void OnLootBoxPurchased(PurchaseLootboxResult result)
        {
            if (result.IsSuccess)
            {
                new PopupViewer().ShowSimplePopup(new PopupRequest
                {
                    Title = ItemTXTHandler.PurchaseTitle,
                    Body = ItemTXTHandler.PurchaseBody
                });
            }
            else
            {
                Debug.Log("Failed to purchase item. " + result.Error.Message);
                new PopupViewer().ShowFabError(result.Error);
            }
        }
    }
}

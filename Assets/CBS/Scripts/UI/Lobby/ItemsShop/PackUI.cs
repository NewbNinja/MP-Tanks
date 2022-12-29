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
    public class PackUI : MonoBehaviour, IScrollableItem<CBSBaseItem>
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

        private CBSItemsPack Pack { get; set; }
        private ItemsIcons ItemIcons { get; set; }
        private CommonPrefabs Prefabs { get; set; }
        private ICBSItems CBSItems { get; set; }

        private List<GameObject> CurrencyPool { get; set; } = new List<GameObject>();
        private List<GameObject> BundlePool { get; set; } = new List<GameObject>();

        private void Awake()
        {
            ItemIcons = CBSScriptable.Get<ItemsIcons>();
            Prefabs = CBSScriptable.Get<CommonPrefabs>();
            CBSItems = CBSModule.Get<CBSItems>();
        }

        public void Display(CBSBaseItem data)
        {
            Pack = data as CBSItemsPack;
            // draw icons
            Icon.sprite = ItemIcons.GetSprite(Pack.ID);
            // draw name
            Title.text = Pack.DisplayName;
            // draw description
            Description.text = Pack.Description;
            // display prices
            foreach (var obj in CurrencyPool)
                obj.SetActive(false);

            var currencies = Pack.Prices;
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
            // draw bundles
            foreach (var obj in BundlePool)
                obj.SetActive(false);

            var bundle = Pack.PackItemsIDs;
            var bundleCurrency = Pack.PackCurrecnies;

            for (int i = 0; i < bundle.Count; i++)
            {
                var itemDI = bundle.ElementAt(i);

                if (i >= BundlePool.Count)
                {
                    var iconPrefab = Prefabs.SimpleIcon;
                    var bundleUI = Instantiate(iconPrefab, BundleRoot);
                    BundlePool.Add(bundleUI);
                    bundleUI.GetComponent<SimpleIcon>().DrawItem(itemDI);
                }
                else
                {
                    BundlePool[i].SetActive(true);
                    BundlePool[i].GetComponent<SimpleIcon>().DrawItem(itemDI);
                }
            }

            for (int i = 0; i < bundleCurrency.Count; i++)
            {
                var co = bundleCurrency.ElementAt(i);
                string currencyID = co.Key;
                int value = (int)co.Value;

                if ((i + bundle.Count) >= BundlePool.Count)
                {
                    var iconPrefab = Prefabs.SimpleIcon;
                    var bundleUI = Instantiate(iconPrefab, BundleRoot);
                    BundlePool.Add(bundleUI);
                    bundleUI.GetComponent<SimpleIcon>().DrawCurrency(currencyID);
                    bundleUI.GetComponent<SimpleIcon>().DrawValue(value.ToString());
                }
                else
                {
                    BundlePool[i + bundle.Count].SetActive(true);
                    BundlePool[i + bundle.Count].GetComponent<SimpleIcon>().DrawCurrency(currencyID);
                    BundlePool[i + bundle.Count].GetComponent<SimpleIcon>().DrawValue(value.ToString());
                }
            }
        }

        // buttons events
        public void Buy(string key, int value)
        {
            string packID = Pack.ID;
            if (string.IsNullOrEmpty(packID))
                return;
            CBSItems.PurchasePack(packID, key, value, OnPackPurchased);
        }

        private void OnPackPurchased(PurchasePackResult result)
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

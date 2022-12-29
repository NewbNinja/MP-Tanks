using CBS.Scriptable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS.UI
{
    public class PrizeDrawer : MonoBehaviour
    {
        private ICBSItems Items { get; set; }
        private ItemsIcons ItemIcons { get; set; }
        private CommonPrefabs Prefabs { get; set; }
        protected virtual GameObject RewardPrefab { get; set; }

        [SerializeField]
        private Transform BundleRoot;

        private List<GameObject> CurrencyPool { get; set; } = new List<GameObject>();
        private List<GameObject> LootPool { get; set; } = new List<GameObject>();

        private void Awake()
        {
            Init();
        }

        protected virtual void Init()
        {
            ItemIcons = CBSScriptable.Get<ItemsIcons>();
            Prefabs = CBSScriptable.Get<CommonPrefabs>();
            Items = CBSModule.Get<CBSItems>();
        }

        public void Display(PrizeObject prizeObject)
        {
            foreach (var obj in CurrencyPool)
                obj.SetActive(false);

            foreach (var obj in LootPool)
                obj.SetActive(false);

            if (prizeObject == null)
                return;

            prizeObject.BundledItems = prizeObject.BundledItems ?? new List<string>();
            prizeObject.Lootboxes = prizeObject.Lootboxes ?? new List<string>();
            prizeObject.BundledVirtualCurrencies = prizeObject.BundledVirtualCurrencies ?? new Dictionary<string, uint>();

            var loot = prizeObject.BundledItems.Concat(prizeObject.Lootboxes).ToList();
            var lootCurrency = prizeObject.BundledVirtualCurrencies;

            // draw items
            for (int i = 0; i < loot.Count; i++)
            {
                var itemID = loot.ElementAt(i);
                var itemObject = Items.GetFromCache(itemID);
                var isItem = itemObject != null && itemObject.Type == ItemType.ITEMS;
                var itemCount = isItem ? (itemObject as CBSItem).UsageCount : 0;

                if (i >= LootPool.Count)
                {
                    var iconPrefab = RewardPrefab ?? Prefabs.SimpleIcon;
                    var bundleUI = Instantiate(iconPrefab, BundleRoot);
                    LootPool.Add(bundleUI);
                    bundleUI.GetComponent<SimpleIcon>().DrawItem(itemID);
                    if (itemCount > 0)
                    {
                        bundleUI.GetComponent<SimpleIcon>().DrawValue(itemCount.ToString());
                    }
                    else
                    {
                        bundleUI.GetComponent<SimpleIcon>().HideValue();
                    }
                }
                else
                {
                    LootPool[i].SetActive(true);
                    LootPool[i].GetComponent<SimpleIcon>().DrawItem(itemID);
                    if (itemCount > 0)
                    {
                        LootPool[i].GetComponent<SimpleIcon>().DrawValue(itemCount.ToString());
                    }
                    else
                    {
                        LootPool[i].GetComponent<SimpleIcon>().HideValue();
                    }
                    
                }
            }

            // draw currency
            for (int i = 0; i < lootCurrency.Count; i++)
            {
                var co = lootCurrency.ElementAt(i);
                string currencyID = co.Key;
                int value = (int)co.Value;

                if ((i + loot.Count) >= LootPool.Count)
                {
                    var iconPrefab = RewardPrefab ?? Prefabs.SimpleIcon;
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
    }
}

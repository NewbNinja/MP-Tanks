using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS.UI
{
    public class LootBoxResult : MonoBehaviour
    {
        [SerializeField]
        private Transform CurrencyRoot;
        [SerializeField]
        private Transform BundleRoot;

        private ItemsIcons ItemIcons { get; set; }
        private CommonPrefabs Prefabs { get; set; }

        private List<GameObject> CurrencyPool { get; set; } = new List<GameObject>();
        private List<GameObject> LootPool { get; set; } = new List<GameObject>();

        private void Awake()
        {
            ItemIcons = CBSScriptable.Get<ItemsIcons>();
            Prefabs = CBSScriptable.Get<CommonPrefabs>();
        }

        public void Display(OpenLootboxResult result)
        {
            foreach (var obj in CurrencyPool)
                obj.SetActive(false);

            foreach (var obj in LootPool)
                obj.SetActive(false);

            var loot = result.GrantedItems;
            var lootCurrency = result.Currencies;

            for (int i = 0; i < loot.Count; i++)
            {
                var itemDI = loot.ElementAt(i).ID;

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
                    var bundleUI = Instantiate(iconPrefab, CurrencyRoot);
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

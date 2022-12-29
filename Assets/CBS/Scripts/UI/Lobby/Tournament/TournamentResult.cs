using CBS.Scriptable;
using CBS.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class TournamentResult : MonoBehaviour
    {
        [SerializeField]
        private Text PlaceLabel;
        [SerializeField]
        private Transform CurrencyRoot;
        [SerializeField]
        private Transform BundleRoot;

        private ItemsIcons ItemIcons { get; set; }
        private CommonPrefabs Prefabs { get; set; }

        private List<GameObject> CurrencyPool { get; set; } = new List<GameObject>();
        private List<GameObject> LootPool { get; set; } = new List<GameObject>();

        private Action OnOkAction { get; set; }

        private void Awake()
        {
            ItemIcons = CBSScriptable.Get<ItemsIcons>();
            Prefabs = CBSScriptable.Get<CommonPrefabs>();
        }

        public void Display(FinishTournamentResult result, Action okAction)
        {
            OnOkAction = okAction;

            foreach (var obj in CurrencyPool)
                obj.SetActive(false);

            foreach (var obj in LootPool)
                obj.SetActive(false);

            // draw position
            PlaceLabel.text = TournamnetTXTHandler.GetPlaceText(result.Position);

            var prizeObject = result.Prize;

            if (prizeObject == null)
                return;

            prizeObject.BundledItems = prizeObject.BundledItems ?? new List<string>();
            prizeObject.Lootboxes = prizeObject.Lootboxes ?? new List<string>();

            var loot = prizeObject.BundledItems.Concat(prizeObject.Lootboxes).ToList();
            var lootCurrency = prizeObject.BundledVirtualCurrencies;

            // draw items
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

            // draw currency
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

        public void OnOkClick()
        {
            OnOkAction?.Invoke();
            gameObject.SetActive(false);
        }
    }
}

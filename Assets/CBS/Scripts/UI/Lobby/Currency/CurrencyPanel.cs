using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS.UI
{
    public class CurrencyPanel : MonoBehaviour
    {
        [SerializeField]
        private DisplayOptions DisplayOption;
        [SerializeField]
        private string[] SelectedCurrencies;

        private CurrencyPrefabs Prefabs { get; set; }
        private AuthData AuthData { get; set; }
        private ICurrency Currencies { get; set; }

        private List<CurrencyItem> CurrenciesUI = new List<CurrencyItem>();

        private void Start()
        {
            Prefabs = CBSScriptable.Get<CurrencyPrefabs>();
            AuthData = CBSScriptable.Get<AuthData>();
            Currencies = CBSModule.Get<CBSCurrency>();

            Currencies.OnCurrencyUpdated += OnCurrencyUpdated;

            // check cache currencies
            if (AuthData.PreloadCurrency)
            {
                var correctedCurrencies = GetSelectedCurrencies(Currencies.CacheCurrencies);
                SpawnItems(correctedCurrencies);
            }
            else
            {
                Currencies.GetCurrencies(OnCurrenciesGet);
            }
        }

        private void OnDestroy()
        {
            Currencies.OnCurrencyUpdated -= OnCurrencyUpdated;
        }

        private void SpawnItems(Dictionary<string, int> currencies)
        {
            var itemPrefab = Prefabs.CurrencyItem;

            foreach (var items in currencies)
            {
                string code = items.Key;
                int value = items.Value;
                var item = Instantiate(itemPrefab, transform);
                var itemUI = item.GetComponent<CurrencyItem>();
                itemUI.Display(code, value);
                CurrenciesUI.Add(itemUI);
            }
        }

        // events
        private void OnCurrenciesGet(CBSGetCurrenciesResult result)
        {
            if (result.IsSuccess)
            {
                var correctedCurrencies = GetSelectedCurrencies(result.Currencies);
                SpawnItems(correctedCurrencies);
            }
        }

        private void OnCurrencyUpdated(CBSUpdateCurrencyResult result)
        {
            foreach(var ui in CurrenciesUI)
            {
                ui.UpdateCurrency(result.CurrencyCode, result.CurrentValue);
            }
        }

        private Dictionary<string, int> GetSelectedCurrencies(Dictionary<string, int> inputCurrencies)
        {
            if (DisplayOption == DisplayOptions.SELECTED)
            {
                return inputCurrencies.Where(x => SelectedCurrencies.Contains(x.Key)).ToDictionary(x=>x.Key, x=>x.Value);
            }
            return inputCurrencies;
        }
    }

    public enum DisplayOptions
    {
        ALL,
        SELECTED
    }
}

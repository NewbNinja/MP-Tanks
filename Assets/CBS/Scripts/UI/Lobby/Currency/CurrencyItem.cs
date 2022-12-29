using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class CurrencyItem : MonoBehaviour
    {
        [SerializeField]
        private Text ValueTitle;
        [SerializeField]
        private Image IconImage;

        private string CurrencyCode { get; set; }
        private int CurrencyValue { get; set; }

        private CurrencyIcons Icons { get; set; }
        private CurrencyPrefabs Prefabs { get; set; }

        private void Awake()
        {
            Icons = CBSScriptable.Get<CurrencyIcons>();
            Prefabs = CBSScriptable.Get<CurrencyPrefabs>();
        }

        public void Display(string code, int value)
        {
            CurrencyCode = code;
            CurrencyValue = value;
            // draw ui
            ValueTitle.text = CurrencyValue.ToString();
            IconImage.sprite = Icons.GetSprite(CurrencyCode);
        }

        public void UpdateCurrency(string code, int value)
        {
            if (code == CurrencyCode)
            {
                CurrencyValue = value;
                ValueTitle.text = CurrencyValue.ToString();
            }
        }

        public void AddCurrency()
        {
            var shopPrefab = Prefabs.CurrenciesPacks;
            UIView.ShowWindow(shopPrefab);
        }
    }
}

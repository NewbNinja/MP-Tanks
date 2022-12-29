using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class SimpleIcon : MonoBehaviour
    {
        [SerializeField]
        private Image Icon;
        [SerializeField]
        private Text Value;

        private ItemsIcons ItemsIcons { get; set; }
        private CurrencyIcons CurrencyIcons { get; set; }

        private void Awake()
        {
            ItemsIcons = CBSScriptable.Get<ItemsIcons>();
            CurrencyIcons = CBSScriptable.Get<CurrencyIcons>();
        }

        public void DrawItem(string id)
        {
            Icon.sprite = ItemsIcons.GetSprite(id);
        }

        public void DrawCurrency(string id)
        {
            Icon.sprite = CurrencyIcons.GetSprite(id);
        }

        public void DrawValue(string val)
        {
            Value.text = val;
        }

        public void HideValue()
        {
            Value.gameObject.SetActive(false);
        }
    }
}

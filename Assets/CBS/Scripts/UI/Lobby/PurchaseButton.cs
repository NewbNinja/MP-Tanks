using CBS.Scriptable;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    [RequireComponent(typeof(Button))]
    public class PurchaseButton : MonoBehaviour
    {
        [SerializeField]
        private Image CurrencyIcon;
        [SerializeField]
        private Text CurrencyValue;

        private Action<string, int> OnPress { get; set; }

        private string Code { get; set; }
        private int Value { get; set; }

        private Button Button { get; set; }
        private CurrencyIcons CurrencyIcons { get; set; }

        private void Awake()
        {
            Button = gameObject.GetComponent<Button>();
            Button.onClick.AddListener(OnClick);
            CurrencyIcons = CBSScriptable.Get<CurrencyIcons>();
        }

        private void OnDestroy()
        {
            Button.onClick.RemoveAllListeners();
        }

        public void Display(string code, int value, Action<string, int> onPress)
        {
            Code = code;
            Value = value;
            OnPress = onPress;
            // display icon
            CurrencyIcon.sprite = CurrencyIcons.GetSprite(code);
            // display value
            CurrencyValue.text = Value.ToString();
        }

        private void OnClick()
        {
            OnPress?.Invoke(Code, Value);
        }
    }
}

using CBS.Scriptable;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class LootBoxSlot : MonoBehaviour
    {
        [SerializeField]
        private Image Icon;

        private ItemsIcons ItemIcons { get; set; }
        private CBSInventoryItem Box { get; set; }

        private Action<CBSInventoryItem> SelectAction { get; set; }

        private Toggle Toggle { get; set; }

        private void Awake()
        {
            ItemIcons = CBSScriptable.Get<ItemsIcons>();
            Toggle = gameObject.GetComponentInChildren<Toggle>();
            Toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnDestroy()
        {
            Toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        public void Configurate(CBSInventoryItem item, ToggleGroup group)
        {
            Box = item;
            // draw icon
            var sprite = ItemIcons.GetSprite(Box.ID);
            Icon.sprite = sprite;
            Toggle.group = group;
        }

        public void SetSelectAction(Action<CBSInventoryItem> action)
        {
            SelectAction = action;
        }

        private void OnToggleValueChanged(bool val)
        {
            if (val)
            {
                SelectAction?.Invoke(Box);
            }
        }

        public void SetToggleValue(bool val)
        {
            Toggle.isOn = val;
        }
    }
}

using CBS.Core;
using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class RouletteSlot : MonoBehaviour, IScrollableItem<RoulettePosition>
    {
        [SerializeField]
        private Image Icon;
        [SerializeField]
        private Image Background;

        private ItemsIcons Icons { get; set; }

        private RoulettePosition Position { get; set; }

        public string ID => Position.ID;

        private void Awake()
        {
            Icons = CBSScriptable.Get<ItemsIcons>();
        }

        public void Display(RoulettePosition data)
        {
            Position = data;
            var id = Position.ID;
            Icon.sprite = Icons.GetSprite(id);
            ToDefault();
        }

        public void ToDefault()
        {
            Background.enabled = false;
        }

        public void Activate()
        {
            Background.enabled = true;
        }
    }
}

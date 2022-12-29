using CBS.Scriptable;
using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class InventorySlot : MonoBehaviour
    {
        [SerializeField]
        private Image Icon;
        [SerializeField]
        private GameObject CounterBack;
        [SerializeField]
        private Text Counter;

        private ItemsIcons ItemIcons { get; set; }
        private CBSInventoryItem Item { get; set; }

        private ICBSInventory CBSInventory { get; set; }

        private void Awake()
        {
            CBSInventory = CBSModule.Get<CBSInventory>();
            ItemIcons = CBSScriptable.Get<ItemsIcons>();
        }

        public void Init(CBSInventoryItem item)
        {
            Item = item;
            // draw icon
            var sprite = ItemIcons.GetSprite(Item.ID);
            Icon.sprite = sprite;
            // draw count
            bool hasCount = item.Count != null && item.Count != 0;
            CounterBack.SetActive(hasCount);
            Counter.text = hasCount ? item.Count.ToString() : string.Empty;
        }

        public void ClickSlot()
        {
            if (Item.IsConsumable)
            {
                ConsumeItem();
            }
            else if (Item.IsEquippable)
            {
                if (Item.Equipped)
                {
                    UnEquipItem();
                }
                else
                {
                    EquipItem();
                }
            }
        }

        private void ConsumeItem()
        {
            new PopupViewer().ShowYesNoPopup(new YesNoPopupRequest {
                Title = ItemTXTHandler.RequestConsumeTitle,
                Body = ItemTXTHandler.RequestConsumeBody,
                OnYesAction = ()=> {
                    CBSInventory.ConsumeItem(Item.InventoryID, result => {
                        if (result.IsSuccess)
                        {
                            if (result.CountLeft > 0)
                            {
                                // change counter
                                Counter.text = result.CountLeft.ToString();
                            }
                            else
                            {
                                // hide in invertory
                                gameObject.SetActive(false);
                            }
                        }
                    });
                }
            });
        }

        private void EquipItem()
        {
            new PopupViewer().ShowYesNoPopup(new YesNoPopupRequest
            {
                Title = ItemTXTHandler.RequestEquipTitle,
                Body = ItemTXTHandler.RequestEquipBody,
                OnYesAction = () => {
                    CBSInventory.EquipItem(Item.InventoryID, result => {
                        if (result.IsSuccess)
                        {
                            gameObject.SetActive(false);
                        }
                    });
                }
            });
        }

        private void UnEquipItem()
        {
            new PopupViewer().ShowYesNoPopup(new YesNoPopupRequest
            {
                Title = ItemTXTHandler.RequestUnEquipTitle,
                Body = ItemTXTHandler.RequestUnEquipBody,
                OnYesAction = () => {
                    CBSInventory.UnEquipItem(Item.InventoryID, result => {
                        if (result.IsSuccess)
                        {
                            gameObject.SetActive(false);
                        }
                    });
                }
            });
        }
    }
}

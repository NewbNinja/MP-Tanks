using CBS.Scriptable;
using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class CurrencyPackItem : MonoBehaviour
    {
        [SerializeField]
        private Text Title;
        [SerializeField]
        private Text Description;
        [SerializeField]
        private Image Icon;
        [SerializeField]
        private Text Price;

        [SerializeField]
        private Transform CurrencySlots;

        private CurrencyPack Pack { get; set; }
        private CurrencyPrefabs Prefabs { get; set; }
        private ICurrency Currency { get; set; }

        private void Awake()
        {
            Prefabs = CBSScriptable.Get<CurrencyPrefabs>();
            Currency = CBSModule.Get<CBSCurrency>();
        }

        public void Display(CurrencyPack pack)
        {
            Pack = pack;
            Title.text = Pack.DisplayName;
            Description.text = Pack.Description;
            Icon.sprite = Pack.IconSprite;
            Price.text = Pack.PriceTitle;

            foreach (var currency in Pack.Currencies)
            {
                var slotPrefab = Prefabs.CurrencySlot;
                var slot = Instantiate(slotPrefab, CurrencySlots);
                slot.GetComponent<CurrencyItem>().Display(currency.Key, (int)currency.Value);
            }
        }

        // button events
        public void PurchasePack()
        {
            Currency.GrantPack(Pack.PackID, result => {
                if (!result.IsSuccess)
                {
                    Debug.Log("Failed to grand pack");
                }
                else
                {
                    new PopupViewer().ShowSimplePopup(new PopupRequest
                    {
                        Title = ItemTXTHandler.PurchaseTitle,
                        Body = ItemTXTHandler.PurchaseBody
                    });
                }
            });
        }
    }
}

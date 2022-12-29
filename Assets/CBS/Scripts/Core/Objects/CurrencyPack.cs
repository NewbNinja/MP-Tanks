using CBS.Scriptable;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS
{
    public class CurrencyPack
    {
        public string PackID { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }
        public string PriceTitle { get; private set; }
        public string ExternalURL { get; private set; }
        public Dictionary<string, uint> Currencies { get; private set; }

        public string Tag {
            get
            {
                bool exist = Detail.Tags != null && Detail.Tags.Count != 0;
                return exist ? Detail.Tags.FirstOrDefault() : string.Empty;
            }
        }

        public Sprite IconSprite {
            get
            {
                return CBSScriptable.Get<CurrencyIcons>().GetSprite(PackID);
            }
        }

        public CatalogItem Detail { get; private set; }

        public CurrencyPack(CatalogItem pack)
        {
            Detail = pack;
            PackID = pack.ItemId;
            DisplayName = pack.DisplayName;
            Description = pack.Description;
            PriceTitle = pack.CustomData;
            ExternalURL = pack.ItemImageUrl;
            Currencies = pack.Bundle.BundledVirtualCurrencies;
        }
    }
}

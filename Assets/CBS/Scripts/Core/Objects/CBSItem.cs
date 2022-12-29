using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS
{
    public class CBSItem : CBSBaseItem
    {
        public bool IsConsumable { get; private set; }
        public bool IsStackable { get; private set; }
        public bool IsTradable { get; private set; }
        public bool HasLifeTime { get; private set; }
        public int UsageCount { get; private set; }
        public int LifeTime { get; private set; }
        public bool IsEquippable { get; private set; }

        public CBSItem (CatalogItem item)
        {
            bool tagExist = item.Tags != null && item.Tags.Count != 0;

            ID = item.ItemId;
            DisplayName = item.DisplayName;
            Description = item.Description;
            Category = tagExist ? item.Tags[0] : CBSConstants.UndefinedCategory;
            IsConsumable = item.Consumable != null && item.Consumable.UsageCount > 0;
            UsageCount = item.Consumable == null || item.Consumable.UsageCount == null ? 0 : (int)item.Consumable.UsageCount;
            IsStackable = item.IsStackable;
            IsTradable = item.IsTradable;
            HasLifeTime = item.Consumable != null && item.Consumable.UsagePeriod != null;
            LifeTime = item.Consumable == null || item.Consumable.UsagePeriod == null ? 0 : (int)item.Consumable.UsagePeriod;
            ExternalIconURL = item.ItemImageUrl;
            Prices = item.VirtualCurrencyPrices;
            ItemClass = item.ItemClass;
            CustomData = item.CustomData;

            var baseData = GetCustomData<CBSItemData>();
            IsEquippable = baseData == null ? false : baseData.IsEquippable;
            Type = baseData == null ? ItemType.ITEMS : baseData.ItemType;
        }
    }
}

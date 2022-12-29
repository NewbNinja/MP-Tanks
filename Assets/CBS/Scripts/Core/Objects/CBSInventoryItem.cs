using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS
{
    public class CBSInventoryItem : CBSBaseItem
    {
        public string InventoryID { get; private set; }
        public bool IsEquippable { get; private set; }
        public bool IsConsumable { get; private set; }
        public bool IsTradable { get; private set; }
        public bool HasLifeTime { get; private set; }
        public DateTime? Expiration { get; private set; }
        public DateTime? PurchaseDate { get; private set; }
        public int? Count { get; private set; }
        public int PurchasePrice { get; private set; }
        public string PurchaseCurrency { get; private set; }
        public PurchaseType PurchaseAction { get; private set; }

        public bool Equipped { get; private set; }

        internal bool IsInTrading { get; private set; }

        private CBSBaseItem BaseItem { get; set; }
        private string BaseDataRaw { get; set; }

        private Dictionary<string, string> InventoryData;

        public CBSInventoryItem(ItemInstance inventoryItem, CBSBaseItem baseItem)
        {
            BaseItem = baseItem;
            ID = inventoryItem.ItemId;
            InventoryID = inventoryItem.ItemInstanceId;
            DisplayName = inventoryItem.DisplayName;
            ItemClass = inventoryItem.ItemClass;
            Expiration = inventoryItem.Expiration;
            PurchaseDate = inventoryItem.PurchaseDate;
            Count = inventoryItem.RemainingUses;
            PurchasePrice = (int)inventoryItem.UnitPrice;
            PurchaseCurrency = inventoryItem.UnitCurrency;
            PurchaseAction = PurchasePrice == 0 && string.IsNullOrEmpty(PurchaseCurrency) ? PurchaseType.GRANDED : PurchaseType.PURCHASED;
            HasLifeTime = Expiration != null;
            InventoryData = inventoryItem.CustomData;

            bool hasEquipData = inventoryItem.CustomData != null && inventoryItem.CustomData.Count > 0 && inventoryItem.CustomData.ContainsKey(CBSConstants.InventoryEqvipedKey);
            bool hasTradeData = inventoryItem.CustomData != null && inventoryItem.CustomData.Count > 0 && inventoryItem.CustomData.ContainsKey(CBSConstants.InventoryTradeKey);
            bool hasBaseData = inventoryItem.CustomData != null && inventoryItem.CustomData.Count > 0 && inventoryItem.CustomData.ContainsKey(CBSConstants.InventoryBaseDataKey);

            if (baseItem != null)
            {
                Category = baseItem.Category;
                Description = baseItem.Description;
                ExternalIconURL = baseItem.ExternalIconURL;
                Prices = baseItem.Prices;
                CustomData = baseItem.CustomData;
                ItemClass = baseItem.ItemClass;
            }

            var baseData = GetCustomData<CBSItemData>();
            IsEquippable = baseData == null ? false : baseData.IsEquippable;
            Type = baseData == null ? ItemType.ITEMS : baseData.ItemType;
            IsConsumable = baseData == null ? false : baseData.IsConsumable;
            IsTradable = baseData == null ? false : baseData.IsTradable;

            Equipped = hasEquipData ? bool.Parse(inventoryItem.CustomData[CBSConstants.InventoryEqvipedKey]) : false;
            IsInTrading = hasTradeData ? bool.Parse(inventoryItem.CustomData[CBSConstants.InventoryTradeKey]) : false;
            BaseDataRaw = hasBaseData ? inventoryItem.CustomData[CBSConstants.InventoryBaseDataKey] : string.Empty;
        }

        public Dictionary<string, string> GetInventoryData()
        {
            return InventoryData == null ? new Dictionary<string, string>() : InventoryData;
        }

        public string GetInventoryDataByKey(string key)
        {
            return InventoryData == null || !InventoryData.ContainsKey(key) ? string.Empty : InventoryData[key];
        }

        public override T GetCustomData<T>()
        {
            if (string.IsNullOrEmpty(BaseDataRaw))
            {
                return BaseItem == null ? null : BaseItem.GetCustomData<T>();
            }
            else
            {
                var dataObject = JsonUtility.FromJson<T>(BaseDataRaw);
                return dataObject;
            }
        }
    }

    public enum PurchaseType
    {
        PURCHASED,
        GRANDED
    }
}
using CBS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS
{
    public interface ICBSInventory
    {
        /// <summary>
        /// Notify when item was consumed from inventory.
        /// </summary>
        event Action<ConsumeInventoryItemResult> OnItemConsumed;
        /// <summary>
        /// Notify when item was equipped.
        /// </summary>
        event Action<EquipInventoryItemResult> OnItemEquiped;
        /// <summary>
        /// Notify when item was unequipped.
        /// </summary>
        event Action<EquipInventoryItemResult> OnItemUnEquiped;
        /// <summary>
        /// Notify when item was added to inventory.
        /// </summary>
        event Action<InventoryItemGrandResult> OnItemAdded;
        /// <summary>
        /// Notify when loot box was added to inventory.
        /// </summary>
        event Action<InventoryLootboxGrandResult> OnLootboxAdded;
        /// <summary>
        /// Notifies when a user has opened a lootbox
        /// </summary>
        event Action<OpenLootboxResult> OnLootboxOpen;

        /// <summary>
        /// Get inventory items list of current user.
        /// </summary>
        /// <param name="OnGetResult"></param>
        void GetInventory(Action<GetInventoryResult> OnGetResult);

        /// <summary>
        /// Get inventory items list of current user by specific category.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="OnGetResult"></param>
        void GetInventoryByCategory(string category, Action<GetInventoryResult> OnGetResult);

        /// <summary>
        /// Consume inventory item by item instance id.
        /// </summary>
        /// <param name="inventoryItemId"></param>
        /// <param name="result"></param>
        void ConsumeItem(string inventoryItemId, Action<ConsumeInventoryItemResult> result);

        /// <summary>
        /// Consume inventory item by item instance id.
        /// </summary>
        /// <param name="inventoryItemId"></param>
        /// <param name="count"></param>
        /// <param name="result"></param>
        void ConsumeItem(string inventoryItemId, int count, Action<ConsumeInventoryItemResult> result);

        /// <summary>
        /// Consume full information of inventory item by instance id.
        /// </summary>
        /// <param name="inventoryItemID"></param>
        /// <param name="result"></param>
        void GetInventoryItem(string inventoryItemID, Action<GetInventoryItemResult> result);

        /// <summary>
        /// Set unique data for item. For example, ID cells in the inventory. Not to be confused with Item Custom Data.
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="dataKey"></param>
        /// <param name="dataValue"></param>
        /// <param name="result"></param>
        void SetInventoryItemData(string inventoryId, string dataKey, string dataValue, Action<SetInventoryDataResult> result);

        /// <summary>
        /// Update custom base item data for a specific item in inventory. For example, you have a sword and want to improve it in the forge
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="inventoryId"></param>
        /// <param name="result"></param>
        void UpdateItemCustomData<T>(T data, string inventoryId, Action<SetInventoryDataResult> result) where T : CBSItemData;

        /// <summary>
        /// Equip item from inventory.
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="result"></param>
        void EquipItem(string inventoryId, Action<EquipInventoryItemResult> result);

        /// <summary>
        /// Unequip item from inventory
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="result"></param>
        void UnEquipItem(string inventoryId, Action<EquipInventoryItemResult> result);

        /// <summary>
        /// Get loot bob list from inventory.
        /// </summary>
        /// <param name="result"></param>
        void GetLootboxes(Action<GetInventoryLootboxesResult> result);

        /// <summary>
        /// Get loot bob list from inventory by category.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="result"></param>
        void GetLootboxesByCategory(string category, Action<GetInventoryLootboxesResult> result);

        /// <summary>
        /// Open loot box and get reward.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="result"></param>
        void OpenLootbox(string instanceId, Action<OpenLootboxResult> result);

        /// <summary>
        /// Get last cached inventory.
        /// </summary>
        /// <returns></returns>
        GetInventoryResult GetInventoryFromCache();

        /// <summary>
        /// Get last cached loot box list
        /// </summary>
        /// <returns></returns>
        GetInventoryLootboxesResult GetLootboxesFromCache();

        /// <summary>
        /// Get specific item from cache inventory.
        /// </summary>
        /// <param name="inventoryItemId"></param>
        /// <returns></returns>
        CBSInventoryItem GetInventoryItemFromCache(string inventoryItemId);
    }
}

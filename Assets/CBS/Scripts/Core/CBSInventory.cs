using CBS.Playfab;
using CBS.Scriptable;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS
{
    public class CBSInventory : CBSModule, ICBSInventory
    {
        private readonly string ItemsCatalog = CBSConstants.ItemsCatalogID;

        /// <summary>
        /// Notify when item was consumed from inventory.
        /// </summary>
        public event Action<ConsumeInventoryItemResult> OnItemConsumed;
        /// <summary>
        /// Notify when item was equipped.
        /// </summary>
        public event Action<EquipInventoryItemResult> OnItemEquiped;
        /// <summary>
        /// Notify when item was unequipped.
        /// </summary>
        public event Action<EquipInventoryItemResult> OnItemUnEquiped;
        /// <summary>
        /// Notify when item was added to inventory.
        /// </summary>
        public event Action<InventoryItemGrandResult> OnItemAdded;
        /// <summary>
        /// Notify when loot box was added to inventory.
        /// </summary>
        public event Action<InventoryLootboxGrandResult> OnLootboxAdded;
        /// <summary>
        /// Notifies when a user has opened a lootbox
        /// </summary>
        public event Action<OpenLootboxResult> OnLootboxOpen;

        private IAuth Auth { get; set; }
        private IFabInventory FabInventory { get; set; }
        private AuthData AuthData { get; set; }

        private List<CBSInventoryItem> CacheInventory { get; set; } = new List<CBSInventoryItem>();

        protected override void Init()
        {
            Auth = Get<CBSAuth>();
            FabInventory = FabExecuter.Get<FabInventory>();
            AuthData = CBSScriptable.Get<AuthData>();
            Auth.OnLoginEvent += OnLoginSuccess;
        }

        // public API

        /// <summary>
        /// Get inventory items list of current user.
        /// </summary>
        /// <param name="OnGetResult"></param>
        public void GetInventory(Action<GetInventoryResult> result)
        {
            FabInventory.GetInventory(OnGet => 
            {
                var items = ParseInventory(OnGet.Inventory);
                items = items.Where(x => x.Type == ItemType.ITEMS).ToList();
                var callback = GenerateInventoryCallback(items);
                result?.Invoke(callback);
            }, OnFailed => {
                var callback = new GetInventoryResult { 
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(OnFailed)
                };
                result?.Invoke(callback);
            });
        }

        /// <summary>
        /// Get inventory items list of current user by specific category.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="OnGetResult"></param>
        public void GetInventoryByCategory(string category, Action<GetInventoryResult> result)
        {
            FabInventory.GetInventory(OnGet =>
            {
                var items = ParseInventory(OnGet.Inventory);
                items = items.Where(x => x.Type == ItemType.ITEMS && x.Category == category).ToList();
                var callback = GenerateInventoryCallback(items);
                result?.Invoke(callback);
            }, OnFailed => {
                var callback = new GetInventoryResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(OnFailed)
                };
                result?.Invoke(callback);
            });
        }

        /// <summary>
        /// Consume full information of inventory item by instance id.
        /// </summary>
        /// <param name="inventoryItemID"></param>
        /// <param name="result"></param>
        public void GetInventoryItem(string inventoryItemID, Action<GetInventoryItemResult> result)
        {
            FabInventory.GetInventory(OnGet =>
            {
                var items = ParseInventory(OnGet.Inventory);
                var item = items.FirstOrDefault(x => x.Type == ItemType.ITEMS && x.InventoryID == inventoryItemID);
                result?.Invoke(new GetInventoryItemResult {
                    IsSuccess = true,
                    InventoryItem = item
                });
            }, OnFailed => {
                var callback = new GetInventoryItemResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(OnFailed)
                };
                result?.Invoke(callback);
            });
        }

        /// <summary>
        /// Consume inventory item by item instance id.
        /// </summary>
        /// <param name="inventoryItemId"></param>
        /// <param name="result"></param>
        public void ConsumeItem(string inventoryItemId, Action<ConsumeInventoryItemResult> result)
        {
            FabInventory.ConsumeItem(inventoryItemId, 1, onConsume => {
                result?.Invoke(new ConsumeInventoryItemResult {
                    IsSuccess = true,
                    InventoryItemId = onConsume.ItemInstanceId,
                    CountLeft = onConsume.RemainingUses
                });
            }, onFailed => {
                result?.Invoke(new ConsumeInventoryItemResult {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Consume inventory item by item instance id.
        /// </summary>
        /// <param name="inventoryItemId"></param>
        /// <param name="count"></param>
        /// <param name="result"></param>
        public void ConsumeItem(string inventoryItemId, int count, Action<ConsumeInventoryItemResult> result)
        {
            FabInventory.ConsumeItem(inventoryItemId, count, onConsume => {
                var callback = new ConsumeInventoryItemResult
                {
                    IsSuccess = true,
                    InventoryItemId = onConsume.ItemInstanceId,
                    CountLeft = onConsume.RemainingUses
                };
                result?.Invoke(callback);
                OnItemConsumed?.Invoke(callback);
            }, onFailed => {
                result?.Invoke(new ConsumeInventoryItemResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Get loot bob list from inventory.
        /// </summary>
        /// <param name="result"></param>
        public void GetLootboxes(Action<GetInventoryLootboxesResult> result)
        {
            FabInventory.GetInventory(OnGet =>
            {
                var items = ParseInventory(OnGet.Inventory);
                items = items.Where(x => x.Type == ItemType.LOOT_BOXES).ToList();
                var callback = GenerateLootboxCallback(items);
                result?.Invoke(callback);
            }, OnFailed => {
                var callback = new GetInventoryLootboxesResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(OnFailed)
                };
                result?.Invoke(callback);
            });
        }

        /// <summary>
        /// Get loot bob list from inventory by category.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="result"></param>
        public void GetLootboxesByCategory(string category, Action<GetInventoryLootboxesResult> result)
        {
            FabInventory.GetInventory(OnGet =>
            {
                var items = ParseInventory(OnGet.Inventory);
                items = items.Where(x => x.Type == ItemType.LOOT_BOXES && x.Category == category).ToList();
                var callback = GenerateLootboxCallback(items);
                result?.Invoke(callback);
            }, OnFailed => {
                var callback = new GetInventoryLootboxesResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(OnFailed)
                };
                result?.Invoke(callback);
            });
        }

        /// <summary>
        /// Equip item from inventory.
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="result"></param>
        public void EquipItem(string inventoryId, Action<EquipInventoryItemResult> result)
        {
            InternalEquipUnequipItem(inventoryId, true, result);
        }

        /// <summary>
        /// Unequip item from inventory
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="result"></param>
        public void UnEquipItem(string inventoryId, Action<EquipInventoryItemResult> result)
        {
            InternalEquipUnequipItem(inventoryId, false, result);
        }

        private void InternalEquipUnequipItem(string inventoryId, bool equip, Action<EquipInventoryItemResult> result)
        {
            FabInventory.SetItemCustomData(new UpdateInventoryCustomDataRequest
            {
                InventoryItemID = inventoryId,
                DataKey = CBSConstants.InventoryEqvipedKey,
                DataValue = equip.ToString()
            }, onSuccess => {
                if (onSuccess.Error != null)
                {
                    var callback = new EquipInventoryItemResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onSuccess.Error)
                    };
                    result?.Invoke(callback);
                }
                else
                {
                    var callback = new EquipInventoryItemResult
                    {
                        IsSuccess = true,
                        InventoryItemId = inventoryId
                    };
                    if (equip)
                    {
                        OnItemEquiped?.Invoke(callback);
                    }
                    else
                    {
                        OnItemUnEquiped?.Invoke(callback);
                    }
                    result?.Invoke(callback);
                }
            }, onFailed => {
                var callback = new EquipInventoryItemResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                };
                result?.Invoke(callback);
            });
        }

        /// <summary>
        /// Set unique data for item. For example, ID cells in the inventory. Not to be confused with Item Custom Data.
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="dataKey"></param>
        /// <param name="dataValue"></param>
        /// <param name="result"></param>
        public void SetInventoryItemData(string inventoryId, string dataKey, string dataValue, Action<SetInventoryDataResult> result)
        {
            FabInventory.SetItemCustomData(new UpdateInventoryCustomDataRequest
            {
                InventoryItemID = inventoryId,
                DataKey = dataKey,
                DataValue = dataValue
            }, onSuccess => {
                if (onSuccess.Error != null)
                {
                    var callback = new SetInventoryDataResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onSuccess.Error)
                    };
                    result?.Invoke(callback);
                }
                else
                {
                    var callback = new SetInventoryDataResult
                    {
                        IsSuccess = true,
                        InventoryItemId = inventoryId
                    };
                    result?.Invoke(callback);
                }
            }, onFailed => {
                var callback = new SetInventoryDataResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                };
                result?.Invoke(callback);
            });
        }

        /// <summary>
        /// Update custom base item data for a specific item in inventory. For example, you have a sword and want to improve it in the forge
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="inventoryId"></param>
        /// <param name="result"></param>
        public void UpdateItemCustomData<T>(T data, string inventoryId, Action<SetInventoryDataResult> result) where T : CBSItemData
        {
            var rawData = JsonUtility.ToJson(data);
            SetInventoryItemData(inventoryId, CBSConstants.InventoryBaseDataKey, rawData, result);
        }

        /// <summary>
        /// Open loot box and get reward.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="result"></param>
        public void OpenLootbox(string instanceId, Action<OpenLootboxResult> result)
        {
            FabInventory.UnlockContainer(instanceId, onSuccess => {
                var fabItems = onSuccess.GrantedItems;
                var items = fabItems.Select(x => new CBSInventoryItem(x, Get<CBSItems>().GetFromDictionary(x.ItemId))).ToList();
                var currencies = onSuccess.VirtualCurrency;
                var callback = new OpenLootboxResult {
                    IsSuccess = true,
                    GrantedItems = items,
                    Currencies = currencies
                };
                result?.Invoke(callback);
                OnLootboxOpen?.Invoke(callback);
                // inventory request change
                foreach (var t in items)
                    ChangeRequest(t);
                // currency request change
                Get<CBSCurrency>().ChangeRequest(currencies.Select(x=>x.Key).ToArray());
            }, onFailed => {
                result?.Invoke(new OpenLootboxResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Get last cached inventory.
        /// </summary>
        /// <returns></returns>
        public GetInventoryResult GetInventoryFromCache()
        {
            var items = CacheInventory.Where(x => x.Type == ItemType.ITEMS).ToList();
            return GenerateInventoryCallback(items);
        }

        /// <summary>
        /// Get specific item from cache inventory.
        /// </summary>
        /// <param name="inventoryItemId"></param>
        /// <returns></returns>
        public CBSInventoryItem GetInventoryItemFromCache(string inventoryItemId)
        {
            return CacheInventory.FirstOrDefault(x => x.InventoryID == inventoryItemId);
        }

        /// <summary>
        /// Get last cached loot box list
        /// </summary>
        /// <returns></returns>
        public GetInventoryLootboxesResult GetLootboxesFromCache()
        {
            var items = CacheInventory.Where(x => x.Type == ItemType.LOOT_BOXES).ToList();
            return GenerateLootboxCallback(items);
        }

        // internal

        private List<CBSInventoryItem> ParseInventory(List<ItemInstance> items)
        {
            items = items.Where(x => x.CatalogVersion == ItemsCatalog).ToList();
            var inventoryItems = items.Select(x => new CBSInventoryItem(x, Get<CBSItems>().GetFromDictionary(x.ItemId))).ToList();
            inventoryItems = inventoryItems.Where(x => x.IsInTrading == false).ToList();
            CacheInventory = inventoryItems;
            return inventoryItems;
        }

        private GetInventoryResult GenerateInventoryCallback(List<CBSInventoryItem> items)
        {
            return new GetInventoryResult
            {
                IsSuccess = true,
                AllItems = items,
                EquippableItems = items.Where(x => x.IsEquippable).ToList(),
                TradableItems = items.Where(x => x.IsTradable).ToList(),
                ConsumableItems = items.Where(x => x.IsConsumable).ToList(),
                EquippedItems = items.Where(x => x.Equipped).ToList(),
                NonEquippedItems = items.Where(x => x.Equipped == false).ToList()
            };
        }

        private GetInventoryLootboxesResult GenerateLootboxCallback(List<CBSInventoryItem> items)
        {
            return new GetInventoryLootboxesResult
            {
                IsSuccess = true,
                Lootboxes = items
            };
        }

        internal void ChangeRequest(CBSInventoryItem item)
        {
            if (CacheInventory != null && CacheInventory.FirstOrDefault(x=>x.InventoryID == item.InventoryID) == null)
            {
                CacheInventory.Add(item);
            }
            if (item.Type == ItemType.ITEMS)
            {
                OnItemAdded?.Invoke(new InventoryItemGrandResult
                {
                    IsSuccess = true,
                    InventoryItem = item
                });
            }
            else if (item.Type == ItemType.LOOT_BOXES)
            {
                OnLootboxAdded?.Invoke(new InventoryLootboxGrandResult
                {
                    IsSuccess = true,
                    InventoryItem = item
                });
            }
        }

        protected override void OnLogout()
        {
            CacheInventory = new List<CBSInventoryItem>();
        }

        // callbacks
        private void OnLoginSuccess(CBSLoginResult result)
        {
            if (result.IsSuccess)
            {
                if (AuthData.PreloadInventory)
                {
                    var inventory = result.Result.InfoResultPayload.UserInventory;
                    var items = ParseInventory(inventory);
                }
            }
        }
    }

    public struct GetInventoryResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public List<CBSInventoryItem> AllItems;
        public List<CBSInventoryItem> EquippedItems;
        public List<CBSInventoryItem> NonEquippedItems;
        public List<CBSInventoryItem> EquippableItems;
        public List<CBSInventoryItem> TradableItems;
        public List<CBSInventoryItem> ConsumableItems;
    }

    public struct GetInventoryLootboxesResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public List<CBSInventoryItem> Lootboxes;
    }

    public struct GetInventoryItemResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public CBSInventoryItem InventoryItem;
    }

    public struct ConsumeInventoryItemResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string InventoryItemId;
        public int CountLeft;
    }

    public struct EquipInventoryItemResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string InventoryItemId;
    }

    public struct SetInventoryDataResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string InventoryItemId;
    }

    public struct InventoryItemGrandResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public CBSInventoryItem InventoryItem;
    }

    public struct InventoryLootboxGrandResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public CBSInventoryItem InventoryItem;
    }

    public struct OpenLootboxResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public List<CBSInventoryItem> GrantedItems;
        public Dictionary<string, uint> Currencies;
    }
}

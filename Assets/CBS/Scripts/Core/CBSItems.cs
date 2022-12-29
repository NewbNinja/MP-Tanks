using CBS.Scriptable;
using PlayFab;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CBS.Playfab;
using PlayFab.ClientModels;

namespace CBS
{
    public class CBSItems : CBSModule, ICBSItems
    {
        /// <summary>
        /// Notify when item was purchased.
        /// </summary>
        public event Action<CBSPurchaseItemResult> OnItemPurchased;
        /// <summary>
        /// Notify when items pack was purchased.
        /// </summary>
        public event Action<PurchasePackResult> OnPackPurchased;
        /// <summary>
        /// Notify when loot box was purchased.
        /// </summary>
        public event Action<PurchaseLootboxResult> OnLootboxPurchased;
        /// <summary>
        /// Notify when item was granted to user.
        /// </summary>
        public event Action<GrantItemResult> OnItemGranted;
        /// <summary>
        /// Notify when items pack was granted to user.
        /// </summary>
        public event Action<GrandPackResult> OnPackGranted;
        /// <summary>
        /// Notify when loot box was granted to user.
        /// </summary>
        public event Action<GrandLootboxResult> OnLootboxGranted;

        private readonly string ItemsCatalog = CBSConstants.ItemsCatalogID;

        private readonly string[] CategoriesKeys = new string[] {
            CBSConstants.CategoriesKey,
            CBSConstants.PackCategoriesKey,
            CBSConstants.LootboxesCategoriesKey
        };

        private List<CBSItem> AllItems { get; set; } = new List<CBSItem>();
        private List<CBSItemsPack> AllPacks { get; set; } = new List<CBSItemsPack>();
        private List<CBSLootbox> AllLootboxes { get; set; } = new List<CBSLootbox>();

        private Dictionary<string, CBSBaseItem> ItemsDictionaty = new Dictionary<string, CBSBaseItem>();

        private string[] ItemCategories { get; set; } = new string[] { };
        private string[] PackCategories { get; set; } = new string[] { };
        private string[] LootboxCategories { get; set; } = new string[] { };

        private IFabItems FabItems { get; set; }
        private IFabTitle FabTitle { get; set; }
        private IFabInventory FabInventory { get; set; }

        private bool IsLoaded { get; set; }

        protected override void Init()
        {
            FabItems = FabExecuter.Get<FabItems>();
            FabTitle = FabExecuter.Get<FabTitleData>();
            FabInventory = FabExecuter.Get<FabInventory>();
        }

        // API calls

        /// <summary>
        /// Updates the state of all items from the Playfab database. Do not use unnecessarily. This method is called at login.
        /// </summary>
        /// <param name="result"></param>
        public void FetchAll(Action<FetchAllResult> result)
        {
            // get all items
            FabItems.GetCatalogItems(ItemsCatalog, onGet => {
                ParceItems(onGet);
                // get categories
                FabTitle.GetTitleData(CategoriesKeys, onGetCatgories => {
                    ParceCategories(onGetCatgories);
                    // generate callback
                    var callback = new FetchAllResult
                    {
                        IsSuccess = true,
                        Items = AllItems,
                        Packs = AllPacks,
                        Lootboxes = AllLootboxes,
                        ItemsCategories = ItemCategories,
                        PacksCategories = PackCategories,
                        LootboxCategories = LootboxCategories
                    };
                    result?.Invoke(callback);
                    IsLoaded = true;
                }, onErrorCategories => {
                    var callback = new FetchAllResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onErrorCategories)
                    };
                    result?.Invoke(callback);
                });
            }, onError => {
                var callback = new FetchAllResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                };
                result?.Invoke(callback);
            });
        }

        /// <summary>
        /// Get all categories by specific items type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="result"></param>
        public void GetCategories(ItemType type, Action<GetCategoriesResult> result)
        {
            if (IsLoaded)
            {
                result?.Invoke(new GetCategoriesResult {
                    IsSuccess = true,
                    Categories = GetCategoryFromType(type)
                });
            }
            else
            {
                FetchAll(onGet => {
                    result?.Invoke(new GetCategoriesResult
                    {
                        IsSuccess = onGet.IsSuccess,
                        Categories = GetCategoryFromType(type),
                        Error = onGet.Error
                    });
                });
            }
        }

        /// <summary>
        /// Get all items available from Playfab database.
        /// </summary>
        /// <param name="result"></param>
        public void GetItems(Action<GetItemsResult> result)
        {
            if (IsLoaded)
            {
                result?.Invoke(new GetItemsResult {
                    IsSuccess = true,
                    Items = AllItems
                });
            }
            else
            {
                FetchAll(onGet => {
                    result?.Invoke(new GetItemsResult
                    {
                        IsSuccess = onGet.IsSuccess,
                        Items = AllItems,
                        Error = onGet.Error
                    });
                });
            }
        }

        /// <summary>
        /// Get all items available from Playfab database by category.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="result"></param>
        public void GetItemsByCategory(string category, Action<GetItemsResult> result)
        {
            GetItems(onGet => {
                if (onGet.Items != null)
                {
                    onGet.Items = AllItems.Where(x => x.Category == category).ToList();
                }
                result?.Invoke(onGet);
            });
        }

        /// <summary>
        /// Get specific item information by id.
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="result"></param>
        public void GetItemByID(string itemID, Action<GetItemResult> result)
        {
            if (IsLoaded)
            {
                result?.Invoke(new GetItemResult
                {
                    IsSuccess = true,
                    Item = AllItems.FirstOrDefault(x => x.ID == itemID)
                });
            }
            else
            {
                FetchAll(onGet => {
                    result?.Invoke(new GetItemResult
                    {
                        IsSuccess = onGet.IsSuccess,
                        Item = AllItems.FirstOrDefault(x => x.ID == itemID),
                        Error = onGet.Error
                    });
                });
            }
        }

        /// <summary>
        /// Purchase item by id. The currency will be debited automatically and the item will be added to the inventory.
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="currencyCode"></param>
        /// <param name="currencyValue"></param>
        /// <param name="result"></param>
        public void PurchaseItem(string itemID, string currencyCode, int currencyValue, Action<CBSPurchaseItemResult> result)
        {
            var dataRequest = new CBSPurchaseRequest
            {
                ItemID = itemID,
                Catalog = ItemsCatalog,
                CurrencyCode = currencyCode,
                CurrencyValue = currencyValue
            };

            FabItems.PurchaseItem(dataRequest, onSuccess => {
                var fabItem = onSuccess.Items.FirstOrDefault();
                var baseItem = GetFromDictionary(fabItem.ItemId);
                var item = new CBSInventoryItem(fabItem, baseItem);
                var callback = new CBSPurchaseItemResult
                {
                    IsSuccess = true,
                    PurchasedItem = item
                };
                result?.Invoke(callback);
                OnItemPurchased?.Invoke(callback);
                Get<CBSInventory>().ChangeRequest(item);
                // send request to currency change
                string currencyPurchase = fabItem.UnitCurrency;
                Get<CBSCurrency>().ChangeRequest(currencyPurchase);
            }, onError => {
                var callback = new CBSPurchaseItemResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                };
                result?.Invoke(callback);
            });
        }

        /// <summary>
        /// Add item to user. The item automatically goes into inventory.
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="result"></param>
        public void GrantItem(string itemID, Action<GrantItemResult> result)
        {
            FabItems.GrandItems(new string[] { itemID }, ItemsCatalog, onSuccess => {
                if (onSuccess.Error == null)
                {
                    Debug.Log(onSuccess.FunctionResult);
                    string rawData = onSuccess.FunctionResult.ToString();
                    var grandResult = JsonUtility.FromJson<GrandResult>(rawData);
                    var fabItem = grandResult.ItemGrantResults.FirstOrDefault();
                    var baseItem = GetFromDictionary(fabItem.ItemId);
                    var invertoryItem = new CBSInventoryItem(fabItem, baseItem);
                    var callback = new GrantItemResult
                    {
                        IsSuccess = true,
                        GrantItem = invertoryItem
                    };
                    Get<CBSInventory>().ChangeRequest(invertoryItem);
                    result?.Invoke(callback);
                    OnItemGranted?.Invoke(callback);
                }
                else
                {
                    result?.Invoke(new GrantItemResult { 
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onSuccess.Error)
                    });
                }
            }, onError => {
                result?.Invoke(new GrantItemResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                });
            });
        }

        /// <summary>
        /// Add items to user. The items automatically goes into inventory.
        /// </summary>
        /// <param name="itemsID"></param>
        /// <param name="result"></param>
        public void GrantItems(string [] itemsID, Action<GrantItemsResult> result)
        {
            FabItems.GrandItems(itemsID, ItemsCatalog, onSuccess => {
                if (onSuccess.Error == null)
                {
                    Debug.Log(onSuccess.FunctionResult);
                    string rawData = onSuccess.FunctionResult.ToString();
                    var grandResult = JsonUtility.FromJson<GrandResult>(rawData);
                    var fabItems = grandResult.ItemGrantResults;

                    var invertoryItems = new List<CBSInventoryItem>();

                    foreach(var item in fabItems)
                    {
                        var baseItem = GetFromDictionary(item.ItemId);
                        var invertoryItem = new CBSInventoryItem(item, baseItem);
                        Get<CBSInventory>().ChangeRequest(invertoryItem);
                        invertoryItems.Add(invertoryItem);
                        OnItemGranted?.Invoke(new GrantItemResult { 
                            IsSuccess = true,
                            GrantItem = invertoryItem
                        });
                    }

                    var callback = new GrantItemsResult
                    {
                        IsSuccess = true,
                        GrantItems = invertoryItems
                    };
                    result?.Invoke(callback);
                }
                else
                {
                    result?.Invoke(new GrantItemsResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onSuccess.Error)
                    });
                }
            }, onError => {
                result?.Invoke(new GrantItemsResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                });
            });
        }

        /// <summary>
        /// Get all items packs available from Playfab database.
        /// </summary>
        /// <param name="result"></param>
        public void GetPacks(Action<GetPacksResult> result)
        {
            if (IsLoaded)
            {
                result?.Invoke(new GetPacksResult
                {
                    IsSuccess = true,
                    Packs = AllPacks
                });
            }
            else
            {
                FetchAll(onGet => {
                    result?.Invoke(new GetPacksResult
                    {
                        IsSuccess = onGet.IsSuccess,
                        Packs = AllPacks,
                        Error = onGet.Error
                    });
                });
            }
        }

        /// <summary>
        /// Get all items packs available from Playfab database by category.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="result"></param>
        public void GetPacksByCategory(string category, Action<GetPacksResult> result)
        {
            GetPacks(onGet => {
                if (onGet.Packs != null)
                {
                    onGet.Packs = AllPacks.Where(x => x.Category == category).ToList();
                }
                result?.Invoke(onGet);
            });
        }

        /// <summary>
        /// Get specific items pack information by id.
        /// </summary>
        /// <param name="packID"></param>
        /// <param name="result"></param>
        public void GetPackByID(string packID, Action<GetPackResult> result)
        {
            if (IsLoaded)
            {
                result?.Invoke(new GetPackResult
                {
                    IsSuccess = true,
                    Pack = AllPacks.FirstOrDefault(x => x.ID == packID)
                });
            }
            else
            {
                FetchAll(onGet => {
                    result?.Invoke(new GetPackResult
                    {
                        IsSuccess = onGet.IsSuccess,
                        Pack = AllPacks.FirstOrDefault(x => x.ID == packID),
                        Error = onGet.Error
                    });
                });
            }
        }

        /// <summary>
        /// Purchase items pack by id. The currency will be debited automatically and the items from pack will be added to the inventory.
        /// </summary>
        /// <param name="packID"></param>
        /// <param name="currencyCode"></param>
        /// <param name="currencyValue"></param>
        /// <param name="result"></param>
        public void PurchasePack(string packID, string currencyCode, int currencyValue, Action<PurchasePackResult> result)
        {
            var dataRequest = new CBSPurchaseRequest
            {
                ItemID = packID,
                Catalog = ItemsCatalog,
                CurrencyCode = currencyCode,
                CurrencyValue = currencyValue
            };

            FabItems.PurchaseItem(dataRequest, onSuccess => {
                var fabItems = onSuccess.Items;
                var pack = fabItems.FirstOrDefault(x => x.ItemId == packID);
                fabItems.Remove(pack);
                fabItems.TrimExcess();

                var grandedItem = new List<CBSInventoryItem>();

                foreach (var item in fabItems)
                {
                    var baseItem = GetFromDictionary(item.ItemId);
                    var inventoryItem = new CBSInventoryItem(item, baseItem);
                    grandedItem.Add(inventoryItem);
                    Get<CBSInventory>().ChangeRequest(inventoryItem);
                }

                GetPackByID(packID, getResult => {
                    if (getResult.IsSuccess)
                    {
                        var grandedCurrencies = getResult.Pack.PackCurrecnies;

                        var callback = new PurchasePackResult
                        {
                            IsSuccess = true,
                            GrandedItem = grandedItem,
                            GrandedCurrencies = grandedCurrencies
                        };
                        result?.Invoke(callback);
                        OnPackPurchased?.Invoke(callback);
                        // send request to currency change
                        var currencyToChange = grandedCurrencies.Select(x => x.Key).ToList();
                        string currencyPurchase = pack.UnitCurrency;
                        if (!currencyToChange.Contains(currencyPurchase))
                            currencyToChange.Add(currencyPurchase);
                        Get<CBSCurrency>().ChangeRequest(currencyToChange.ToArray());
                        // remove pack from invertory
                        FabInventory.RemoveInventoryItem(pack.ItemInstanceId, null, null);
                    }
                    else
                    {
                        FailedToPurchasePack(result, getResult.Error);
                    }
                });
            }, onError => {
                FailedToPurchasePack(result, SimpleError.FromTemplate(onError));
            });
        }

        private void FailedToPurchasePack(Action<PurchasePackResult> result, SimpleError error)
        {
            var callback = new PurchasePackResult
            {
                IsSuccess = false,
                Error = error
            };
            result?.Invoke(callback);
        }

        /// <summary>
        /// Add items pack to user. The items from pack automatically goes into inventory.
        /// </summary>
        /// <param name="packID"></param>
        /// <param name="result"></param>
        public void GrantPack(string packID, Action<GrandPackResult> result)
        {
            FabItems.GrandBundle(packID, ItemsCatalog, onSuccess => {
                if (onSuccess.Error == null)
                {
                    string rawData = onSuccess.FunctionResult.ToString();
                    var grandResult = JsonUtility.FromJson<GrandResult>(rawData);
                    var fabItems = grandResult.ItemGrantResults;
                    var pack = fabItems.FirstOrDefault(x => x.ItemId == packID);
                    fabItems.Remove(pack);
                    fabItems.TrimExcess();

                    var grandedItem = new List<CBSInventoryItem>();

                    foreach (var item in fabItems)
                    {
                        var baseItem = GetFromDictionary(item.ItemId);
                        var inventoryItem = new CBSInventoryItem(item, baseItem);
                        grandedItem.Add(inventoryItem);
                        Get<CBSInventory>().ChangeRequest(inventoryItem);
                    }

                    GetPackByID(packID, getResult => {
                        if (getResult.IsSuccess)
                        {
                            var grandedCurrencies = getResult.Pack.PackCurrecnies;

                            var callback = new GrandPackResult
                            {
                                IsSuccess = true,
                                GrandedItem = grandedItem,
                                GrandedCurrencies = grandedCurrencies
                            };
                            result?.Invoke(callback);
                            OnPackGranted?.Invoke(callback);
                            // send request to currency change
                            var currencyToChange = grandedCurrencies.Select(x => x.Key).ToList();
                            currencyToChange.TrimExcess();
                            Get<CBSCurrency>().ChangeRequest(currencyToChange.ToArray());
                        }
                        else
                        {
                            FailedToGrandPack(result, getResult.Error);
                        }
                    });
                }
                else
                {
                    FailedToGrandPack(result, SimpleError.FromTemplate(onSuccess.Error));
                }
            }, onError => {
                FailedToGrandPack(result, SimpleError.FromTemplate(onError));
            });
        }

        private void FailedToGrandPack(Action<GrandPackResult> result, SimpleError error)
        {
            var callback = new GrandPackResult
            {
                IsSuccess = false,
                Error = error
            };
            result?.Invoke(callback);
        }

        /// <summary>
        /// Get all loot boxws available from Playfab database.
        /// </summary>
        /// <param name="result"></param>
        public void GetLootboxes(Action<GetLootboxesResult> result)
        {
            if (IsLoaded)
            {
                result?.Invoke(new GetLootboxesResult
                {
                    IsSuccess = true,
                    Lootboxes = AllLootboxes
                });
            }
            else
            {
                FetchAll(onGet => {
                    result?.Invoke(new GetLootboxesResult
                    {
                        IsSuccess = onGet.IsSuccess,
                        Lootboxes = AllLootboxes,
                        Error = onGet.Error
                    });
                });
            }
        }

        /// <summary>
        /// Get all loot boxws available from Playfab database by category.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="result"></param>
        public void GetLootboxesByCategory(string category, Action<GetLootboxesResult> result)
        {
            GetLootboxes(onGet => {
                if (onGet.Lootboxes != null)
                {
                    onGet.Lootboxes = AllLootboxes.Where(x => x.Category == category).ToList();
                }
                result?.Invoke(onGet);
            });
        }

        /// <summary>
        /// Get specific loot box information by id.
        /// </summary>
        /// <param name="lootboxID"></param>
        /// <param name="result"></param>
        public void GetLootboxByID(string lootboxID, Action<GetLootboxResult> result)
        {
            if (IsLoaded)
            {
                result?.Invoke(new GetLootboxResult
                {
                    IsSuccess = true,
                    Lootbox = AllLootboxes.FirstOrDefault(x => x.ID == lootboxID)
                });
            }
            else
            {
                FetchAll(onGet => {
                    result?.Invoke(new GetLootboxResult
                    {
                        IsSuccess = onGet.IsSuccess,
                        Lootbox = AllLootboxes.FirstOrDefault(x => x.ID == lootboxID),
                        Error = onGet.Error
                    });
                });
            }
        }

        /// <summary>
        /// Purchase loot box by id. The currency will be debited automatically and the loot box will be added to the inventory.
        /// </summary>
        /// <param name="lootboxID"></param>
        /// <param name="currencyCode"></param>
        /// <param name="currencyValue"></param>
        /// <param name="result"></param>
        public void PurchaseLootbox(string lootboxID, string currencyCode, int currencyValue, Action<PurchaseLootboxResult> result)
        {
            var dataRequest = new CBSPurchaseRequest
            {
                ItemID = lootboxID,
                Catalog = ItemsCatalog,
                CurrencyCode = currencyCode,
                CurrencyValue = currencyValue
            };

            FabItems.PurchaseItem(dataRequest, onSuccess => {
                var fabItems = onSuccess.Items;
                var lootbox = fabItems.FirstOrDefault(x => x.ItemId == lootboxID);
                var baseItem = GetFromDictionary(lootbox.ItemId);
                var invertoryBox = new CBSInventoryItem(lootbox, baseItem);
                Get<CBSInventory>().ChangeRequest(invertoryBox);
                var callback = new PurchaseLootboxResult
                {
                    IsSuccess = true,
                    PurchasedItem = invertoryBox
                };
                result?.Invoke(callback);
                OnLootboxPurchased?.Invoke(callback);
                // send request to currency change
                string currencyPurchase = lootbox.UnitCurrency;
                Get<CBSCurrency>().ChangeRequest(currencyPurchase);
            }, onError => {
                FailedToPurchaseLuBox(result, SimpleError.FromTemplate(onError));
            });
        }

        private void FailedToPurchaseLuBox(Action<PurchaseLootboxResult> result, SimpleError error)
        {
            var callback = new PurchaseLootboxResult
            {
                IsSuccess = false,
                Error = error
            };
            result?.Invoke(callback);
        }

        /// <summary>
        /// Add loot box to user. The loot box automatically goes into inventory.
        /// </summary>
        /// <param name="lootboxID"></param>
        /// <param name="result"></param>
        public void GrantLootbox(string lootboxID, Action<GrandLootboxResult> result)
        {
            FabItems.GrandItems(new string[] { lootboxID }, ItemsCatalog, onSuccess => {
                if (onSuccess.Error == null)
                {
                    string rawData = onSuccess.FunctionResult.ToString();
                    var grandResult = JsonUtility.FromJson<GrandResult>(rawData);
                    var fabItems = grandResult.ItemGrantResults;
                    var pack = fabItems.FirstOrDefault(x => x.ItemId == lootboxID);
                    var baseItem = GetFromDictionary(pack.ItemId);
                    var invertoryItem = new CBSInventoryItem(pack, baseItem);
                    Get<CBSInventory>().ChangeRequest(invertoryItem);

                    GetLootboxByID(lootboxID, getResult => {
                        if (getResult.IsSuccess)
                        {
                            var grandedCurrencies = getResult.Lootbox.PackCurrecnies;

                            var callback = new GrandLootboxResult
                            {
                                IsSuccess = true,
                                GrandedBox = invertoryItem,
                                GrandedCurrencies = grandedCurrencies
                            };
                            result?.Invoke(callback);
                            OnLootboxGranted?.Invoke(callback);
                            // send request to currency change
                            var currencyToChange = grandedCurrencies.Select(x => x.Key).ToList();
                            currencyToChange.TrimExcess();
                            Get<CBSCurrency>().ChangeRequest(currencyToChange.ToArray());
                        }
                        else
                        {
                            FailedToGrandLootBox(result, getResult.Error);
                        }
                    });
                }
                else
                {
                    FailedToGrandLootBox(result, SimpleError.FromTemplate(onSuccess.Error));
                }
            }, onError => {
                FailedToGrandLootBox(result, SimpleError.FromTemplate(onError));
            });
        }

        private void FailedToGrandLootBox(Action<GrandLootboxResult> result, SimpleError error)
        {
            var callback = new GrandLootboxResult
            {
                IsSuccess = false,
                Error = error
            };
            result?.Invoke(callback);
        }


        /// <summary>
        /// Get item from cached dictionary
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public CBSBaseItem GetFromCache(string itemID)
        {
            return GetFromDictionary(itemID);
        }

        // internal
        private void ParceItems(GetCatalogItemsResult result)
        {
            AllItems = new List<CBSItem>();
            AllPacks = new List<CBSItemsPack>();
            AllLootboxes = new List<CBSLootbox>();
            // parse items
            var items = result.Catalog.Where(x => x.Bundle == null && x.Container == null);
            foreach (var item in items)
            {
                var cbsItem = new CBSItem(item);
                AllItems.Add(cbsItem);
                ItemsDictionaty[item.ItemId] = cbsItem;
            }
            // parse packs
            var packs = result.Catalog.Where(x => x.Bundle != null);
            foreach (var pack in packs)
            {
                var cbsPack = new CBSItemsPack(pack);
                AllPacks.Add(cbsPack);
                ItemsDictionaty[pack.ItemId] = cbsPack;
            }
            // parse loot boxes
            var lootboxes = result.Catalog.Where(x => x.Container != null);
            foreach (var box in lootboxes)
            {
                var cbsLootbox = new CBSLootbox(box);
                AllLootboxes.Add(cbsLootbox);
                ItemsDictionaty[box.ItemId] = cbsLootbox;
            }
        }

        private void ParceCategories(GetTitleDataResult result)
        {
            var data = result.Data;
            if (data.ContainsKey(CBSConstants.CategoriesKey))
            {
                var rawData = data[CBSConstants.CategoriesKey];
                var categoryObject = JsonUtility.FromJson<Categories>(rawData);
                ItemCategories = categoryObject.List.ToArray();
            }
            if (data.ContainsKey(CBSConstants.PackCategoriesKey))
            {
                var rawData = data[CBSConstants.PackCategoriesKey];
                var categoryObject = JsonUtility.FromJson<Categories>(rawData);
                PackCategories = categoryObject.List.ToArray();
            }
            if (data.ContainsKey(CBSConstants.LootboxesCategoriesKey))
            {
                var rawData = data[CBSConstants.LootboxesCategoriesKey];
                var categoryObject = JsonUtility.FromJson<Categories>(rawData);
                LootboxCategories = categoryObject.List.ToArray();
            }
        }

        internal CBSBaseItem GetFromDictionary(string itemID)
        {
            try
            {
                return ItemsDictionaty[itemID];
            }
            catch
            {
                return null;
            }
        }

        private string [] GetCategoryFromType(ItemType type)
        {
            if (type == ItemType.ITEMS)
                return ItemCategories;
            else if (type == ItemType.PACKS)
                return PackCategories;
            else if (type == ItemType.LOOT_BOXES)
                return LootboxCategories;
            return new string[] { };
        }

        protected override void OnLogout()
        {
            AllItems = new List<CBSItem>();
            AllPacks = new List<CBSItemsPack>();
            AllLootboxes = new List<CBSLootbox>();

            ItemsDictionaty = new Dictionary<string, CBSBaseItem>();

            ItemCategories = new string[] { };
            PackCategories = new string[] { };
            LootboxCategories = new string[] { };
        }
    }

    [Serializable]
    public class Categories
    {
        public string TitleKey { get; set; }

        public List<string> List;
    }

    [Serializable]
    public class GrandResult
    {
        public List<ItemInstance> ItemGrantResults;
    }

    public struct GetItemsResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public List<CBSItem> Items;
    }

    public struct GetItemResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public CBSItem Item;
    }

    public struct CBSPurchaseItemResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public CBSInventoryItem PurchasedItem;
    }

    public struct GrantItemResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public CBSInventoryItem GrantItem;
    }

    public struct GrantItemsResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public List<CBSInventoryItem> GrantItems;
    }

    public struct GetPacksResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public List<CBSItemsPack> Packs;
    }

    public struct GetPackResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public CBSItemsPack Pack;
    }

    public struct PurchasePackResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public List<CBSInventoryItem> GrandedItem;
        public Dictionary<string, uint> GrandedCurrencies;
    }

    public struct GrandPackResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public List<CBSInventoryItem> GrandedItem;
        public Dictionary<string, uint> GrandedCurrencies;
    }

    public struct GetLootboxesResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public List<CBSLootbox> Lootboxes;
    }

    public struct GetLootboxResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public CBSLootbox Lootbox;
    }

    public struct PurchaseLootboxResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public CBSInventoryItem PurchasedItem;
    }

    public struct GrandLootboxResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public CBSInventoryItem GrandedBox;
        public Dictionary<string, uint> GrandedCurrencies;
    }

    public struct FetchAllResult
    {
        public bool IsSuccess;
        public SimpleError Error;

        public List<CBSItem> Items;
        public List<CBSItemsPack> Packs;
        public List<CBSLootbox> Lootboxes;

        public string[] ItemsCategories;
        public string[] PacksCategories;
        public string[] LootboxCategories;
    }

    public struct GetCategoriesResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string [] Categories;
    }

    public enum ItemType
    {
        ITEMS = 0,
        PACKS = 1,
        LOOT_BOXES = 2
    }
}
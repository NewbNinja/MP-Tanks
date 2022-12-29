using System;

namespace CBS
{
    public interface ICBSItems
    {
        /// <summary>
        /// Notify when item was purchased.
        /// </summary>
        event Action<CBSPurchaseItemResult> OnItemPurchased;
        /// <summary>
        /// Notify when items pack was purchased.
        /// </summary>
        event Action<PurchasePackResult> OnPackPurchased;
        /// <summary>
        /// Notify when loot box was purchased.
        /// </summary>
        event Action<PurchaseLootboxResult> OnLootboxPurchased;
        /// <summary>
        /// Notify when item was granted to user.
        /// </summary>
        event Action<GrantItemResult> OnItemGranted;
        /// <summary>
        /// Notify when items pack was granted to user.
        /// </summary>
        event Action<GrandPackResult> OnPackGranted;
        /// <summary>
        /// Notify when loot box was granted to user.
        /// </summary>
        event Action<GrandLootboxResult> OnLootboxGranted;

        /// <summary>
        /// Updates the state of all items from the Playfab database. Do not use unnecessarily. This method is called at login.
        /// </summary>
        /// <param name="result"></param>
        void FetchAll(Action<FetchAllResult> result);

        /// <summary>
        /// Get all categories by specific items type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="result"></param>
        void GetCategories(ItemType type, Action<GetCategoriesResult> result);
        /// <summary>
        /// Get all items available from Playfab database.
        /// </summary>
        /// <param name="result"></param>
        void GetItems(Action<GetItemsResult> result);
        /// <summary>
        /// Get all items available from Playfab database by category.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="result"></param>
        void GetItemsByCategory(string category, Action<GetItemsResult> result);
        /// <summary>
        /// Get specific item information by id.
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="result"></param>
        void GetItemByID(string itemID, Action<GetItemResult> result);
        /// <summary>
        /// Purchase item by id. The currency will be debited automatically and the item will be added to the inventory.
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="currencyCode"></param>
        /// <param name="currencyValue"></param>
        /// <param name="result"></param>
        void PurchaseItem(string itemID, string currencyCode, int currencyValue, Action<CBSPurchaseItemResult> result);
        /// <summary>
        /// Add item to user. The item automatically goes into inventory.
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="result"></param>
        void GrantItem(string itemID, Action<GrantItemResult> result);
        /// <summary>
        /// Add items to user. The items automatically goes into inventory.
        /// </summary>
        /// <param name="itemsID"></param>
        /// <param name="result"></param>
        void GrantItems(string[] itemsID, Action<GrantItemsResult> result);

        /// <summary>
        /// Get all items packs available from Playfab database.
        /// </summary>
        /// <param name="result"></param>
        void GetPacks(Action<GetPacksResult> result);
        /// <summary>
        /// Get all items packs available from Playfab database by category.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="result"></param>
        void GetPacksByCategory(string category, Action<GetPacksResult> result);
        /// <summary>
        /// Get specific items pack information by id.
        /// </summary>
        /// <param name="packID"></param>
        /// <param name="result"></param>
        void GetPackByID(string packID, Action<GetPackResult> result);
        /// <summary>
        /// Purchase items pack by id. The currency will be debited automatically and the items from pack will be added to the inventory.
        /// </summary>
        /// <param name="packID"></param>
        /// <param name="currencyCode"></param>
        /// <param name="currencyValue"></param>
        /// <param name="result"></param>
        void PurchasePack(string packID, string currencyCode, int currencyValue, Action<PurchasePackResult> result);
        /// <summary>
        /// Add items pack to user. The items from pack automatically goes into inventory.
        /// </summary>
        /// <param name="packID"></param>
        /// <param name="result"></param>
        void GrantPack(string packID, Action<GrandPackResult> result);

        /// <summary>
        /// Get all loot boxws available from Playfab database.
        /// </summary>
        /// <param name="result"></param>
        void GetLootboxes(Action<GetLootboxesResult> result);
        /// <summary>
        /// Get all loot boxws available from Playfab database by category.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="result"></param>
        void GetLootboxesByCategory(string category, Action<GetLootboxesResult> result);
        /// <summary>
        /// Get specific loot box information by id.
        /// </summary>
        /// <param name="lootboxID"></param>
        /// <param name="result"></param>
        void GetLootboxByID(string lootboxID, Action<GetLootboxResult> result);
        /// <summary>
        /// Purchase loot box by id. The currency will be debited automatically and the loot box will be added to the inventory.
        /// </summary>
        /// <param name="lootboxID"></param>
        /// <param name="currencyCode"></param>
        /// <param name="currencyValue"></param>
        /// <param name="result"></param>
        void PurchaseLootbox(string lootboxID, string currencyCode, int currencyValue, Action<PurchaseLootboxResult> result);
        /// <summary>
        /// Add loot box to user. The loot box automatically goes into inventory.
        /// </summary>
        /// <param name="lootboxID"></param>
        /// <param name="result"></param>
        void GrantLootbox(string lootboxID, Action<GrandLootboxResult> result);
        /// <summary>
        /// Get item from cached dictionary
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        CBSBaseItem GetFromCache(string itemID);
    }
}

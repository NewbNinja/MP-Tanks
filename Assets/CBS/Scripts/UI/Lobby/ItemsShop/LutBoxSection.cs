using CBS.Core;
using CBS.Scriptable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS.UI
{
    public class LootBoxSection : IShopSection
    {
        private ICBSItems Items { get; set; }

        public GameObject uiPrefab { get; set; }

        public LootBoxSection()
        {
            Items = CBSModule.Get<CBSItems>();
            uiPrefab = CBSScriptable.Get<ShopPrefabs>().ShopLootBox;
        }

        public void GetCategories(Action<string[]> categories)
        {
            Items.GetCategories(ItemType.LOOT_BOXES, result => {
                if (result.IsSuccess)
                {
                    categories?.Invoke(result.Categories);
                }
                else
                {
                    categories?.Invoke(new string[] { });
                }
            });
        }

        public void GetItems(Action<List<CBSBaseItem>> items)
        {
            Items.GetLootboxes(result => {
                if (result.IsSuccess)
                {
                    items?.Invoke(result.Lootboxes.Select(x=>x as CBSBaseItem).ToList());
                }
                else
                {
                    items?.Invoke(new List<CBSBaseItem>());
                }
            });
        }

        public void GetItemsByCategory(string category, Action<List<CBSBaseItem>> items)
        {
            Items.GetLootboxesByCategory(category, result => {
                if (result.IsSuccess)
                {
                    items?.Invoke(result.Lootboxes.Select(x => x as CBSBaseItem).ToList());
                }
                else
                {
                    items?.Invoke(new List<CBSBaseItem>());
                }
            });
        }
    }
}

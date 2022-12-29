using CBS.Core;
using CBS.Scriptable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS.UI
{
    public class ItemsSection : IShopSection
    {
        private ICBSItems Items { get; set; }

        public GameObject uiPrefab { get; set; }

        public ItemsSection()
        {
            Items = CBSModule.Get<CBSItems>();
            uiPrefab = CBSScriptable.Get<ShopPrefabs>().ShopItem;
        }

        public void GetCategories(Action<string[]> categories)
        {
            Items.GetCategories(ItemType.ITEMS, result => {
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
            Items.GetItems(result => {
                if (result.IsSuccess)
                {
                    items?.Invoke(result.Items.Select(x=>x as CBSBaseItem).ToList());
                }
                else
                {
                    items?.Invoke(new List<CBSBaseItem>());
                }
            });
        }

        public void GetItemsByCategory(string category, Action<List<CBSBaseItem>> items)
        {
            Items.GetItemsByCategory(category, result => {
                if (result.IsSuccess)
                {
                    var resultList = result.Items.Select(x => x as CBSBaseItem).ToList();
                    items?.Invoke(resultList);
                }
                else
                {
                    items?.Invoke(new List<CBSBaseItem>());
                }
            });
        }
    }
}

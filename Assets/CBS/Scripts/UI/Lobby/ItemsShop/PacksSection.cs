using CBS.Core;
using CBS.Scriptable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS.UI
{
    public class PacksSection : IShopSection
    {
        private ICBSItems Items { get; set; }

        public GameObject uiPrefab { get; set; }

        public PacksSection()
        {
            Items = CBSModule.Get<CBSItems>();
            uiPrefab = CBSScriptable.Get<ShopPrefabs>().ShopPack;
        }

        public void GetCategories(Action<string[]> categories)
        {
            Items.GetCategories(ItemType.PACKS, result => {
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
            Items.GetPacks(result => {
                if (result.IsSuccess)
                {
                    items?.Invoke(result.Packs.Select(x=>x as CBSBaseItem).ToList());
                }
                else
                {
                    items?.Invoke(new List<CBSBaseItem>());
                }
            });
        }

        public void GetItemsByCategory(string category, Action<List<CBSBaseItem>> items)
        {
            Items.GetPacksByCategory(category, result => {
                if (result.IsSuccess)
                {
                    items?.Invoke(result.Packs.Select(x => x as CBSBaseItem).ToList());
                }
                else
                {
                    items?.Invoke(new List<CBSBaseItem>());
                }
            });
        }
    }
}

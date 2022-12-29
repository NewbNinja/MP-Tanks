using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public interface IShopSection
    {
        GameObject uiPrefab { get; }

        void GetCategories(Action<string[]> categories);
        void GetItems(Action<List<CBSBaseItem>> items);
        void GetItemsByCategory(string category, Action<List<CBSBaseItem>> items);
    }
}

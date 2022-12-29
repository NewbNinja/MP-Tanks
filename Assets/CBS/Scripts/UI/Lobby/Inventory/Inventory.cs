using CBS.Core;
using CBS.Scriptable;
using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField]
        private ToggleGroup CategoryGroup;
        [SerializeField]
        private BaseScroller CategoryScroller;
        [SerializeField]
        private BaseScroller ItemsScroller;

        private string[] CurrentCategories { get; set; }
        private List<CBSInventoryItem> CurrentItems { get; set; }
        private string CurrentCategory { get; set; }

        private ICBSItems Items { get; set; }
        private ICBSInventory CBSInventory { get; set; }

        private InventoryPrefabs InventoryPrefabs { get; set; }
        private ShopPrefabs ShopPrefabs { get; set; }

        private void Awake()
        {
            CBSInventory = CBSModule.Get<CBSInventory>();
            Items = CBSModule.Get<CBSItems>();
            InventoryPrefabs = CBSScriptable.Get<InventoryPrefabs>();
            ShopPrefabs = CBSScriptable.Get<ShopPrefabs>();
            // add listeners
            CategoryScroller.OnSpawn += OnCategorySpawned;
            ItemsScroller.OnSpawn += OnItemSpawn;
        }

        private void OnDestroy()
        {
            CategoryScroller.OnSpawn -= OnCategorySpawned;
            ItemsScroller.OnSpawn -= OnItemSpawn;
        }

        private void OnEnable()
        {
            CBSInventory.OnItemUnEquiped += OnItemUnequiped;
            DisplayCategories();
        }

        private void OnDisable()
        {
            CBSInventory.OnItemUnEquiped -= OnItemUnequiped;
        }

        // category
        private void DisplayCategories()
        {
            Items.GetCategories(ItemType.ITEMS, OnCategoriesGetted);
        }

        // draw items
        private void DrawItems()
        {
            int count = CurrentItems == null ? 0 : CurrentItems.Count;
            var slotPrefab = InventoryPrefabs.InventorySlot;
            ItemsScroller.SpawnItems(slotPrefab, count);
        }

        private void OnCategoriesGetted(GetCategoriesResult result)
        {
            CategoryGroup.SetAllTogglesOff();
            CurrentCategories = result.Categories;

            // add ALL tab
            var categoriesList = CurrentCategories.ToList();
            categoriesList.Insert(0, UIUtils.ALL_MENU_TITLE);
            CurrentCategories = categoriesList.ToArray();

            int count = CurrentCategories.Length;
            var categoryPrefab = ShopPrefabs.CategoryTab;
            CategoryScroller.SpawnItems(categoryPrefab, count);
        }

        private void OnCategorySpawned(GameObject uiItem, int index)
        {
            var scroll = CategoryScroller.GetComponent<ScrollRect>();
            float contentWidth = scroll.GetComponent<RectTransform>().sizeDelta.x;
            int categoriesCount = CurrentCategories.Length;
            float tabWidth = contentWidth / categoriesCount;

            var rectComponent = uiItem.GetComponent<RectTransform>();
            rectComponent.sizeDelta = new Vector2(tabWidth, rectComponent.sizeDelta.y);
            var toggleComponent = uiItem.GetComponent<Toggle>();
            toggleComponent.group = CategoryGroup;
            toggleComponent.isOn = false;
            var tabComponent = uiItem.GetComponent<CategoryTab>();
            tabComponent.TabObject = CurrentCategories[index];
            tabComponent.SetSelectAction(OnCategorySelected);
            if (index == 0)
            {
                toggleComponent.isOn = true;
            }
        }

        private void OnCategorySelected(string category)
        {
            // fetch items
            CurrentCategory = category;
            ItemsScroller.Clear();
            if (category == UIUtils.ALL_MENU_TITLE)
                CBSInventory.GetInventory(OnGetInventory);
            else
                CBSInventory.GetInventoryByCategory(category, OnGetInventory);
        }

        private void OnGetInventory(GetInventoryResult result)
        {
            if (result.IsSuccess)
            {
                CurrentItems = result.NonEquippedItems;
                DrawItems();
            }
        }

        private void OnItemUnequiped(EquipInventoryItemResult result)
        {
            if (CurrentCategory == UIUtils.ALL_MENU_TITLE)
                CBSInventory.GetInventory(OnGetInventory);
            else
                CBSInventory.GetInventoryByCategory(CurrentCategory, OnGetInventory);
        }

        private void OnItemSpawn(GameObject uiItem, int index)
        {
            var item = CurrentItems[index];
            var uiComponent = uiItem.GetComponent<InventorySlot>();
            uiComponent.Init(item);
        }
    }
}

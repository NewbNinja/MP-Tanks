using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using CBS.Core;
using CBS.Scriptable;
using CBS.Utils;

namespace CBS.UI
{
    public class ItemsShop : MonoBehaviour
    {
        [SerializeField]
        private Toggle[] ShopTabs;
        [SerializeField]
        private BaseScroller CategoryScroller;
        [SerializeField]
        private ItemsScroller ItemsScroller;
        [SerializeField]
        private ItemsScroller PacksScroller;
        [SerializeField]
        private ItemsScroller LootBoxScroller;
        [SerializeField]
        private ToggleGroup CategoryGroup;

        private IShopSection Section { get; set; }
        private ShopPrefabs Prefabs { get; set; }
        private string [] CurrentCategories { get; set; }

        private ItemsScroller ActiveScroller
        {
            get 
            {
                if (ItemsScroller.gameObject.activeInHierarchy)
                    return ItemsScroller;
                else if (PacksScroller.gameObject.activeInHierarchy)
                    return PacksScroller;
                else if (LootBoxScroller.gameObject.activeInHierarchy)
                    return LootBoxScroller;
                return ItemsScroller;
            }
            set
            {
                ItemsScroller.gameObject.SetActive(value == ItemsScroller);
                PacksScroller.gameObject.SetActive(value == PacksScroller);
                LootBoxScroller.gameObject.SetActive(value == LootBoxScroller);
            }
        }

        private void Awake()
        {
            Prefabs = CBSScriptable.Get<ShopPrefabs>();

            // add listeners
            foreach (var toggle in ShopTabs)
            {
                toggle.onValueChanged.AddListener(OnTabChanged);
            }
            CategoryScroller.OnSpawn += OnCategorySpawned;
        }

        private void OnDestroy()
        {
            // remove listeners
            foreach (var toggle in ShopTabs)
            {
                toggle.onValueChanged.RemoveListener(OnTabChanged);
            }
            CategoryScroller.OnSpawn -= OnCategorySpawned;
        }

        // category
        private void DisplayCategories()
        {
            Section?.GetCategories(OnCategoriesGetted);
        }

        private void OnCategoriesGetted(string[] categories)
        {
            // add ALL tab
            var categoriesList = categories.ToList();
            categoriesList.Insert(0, UIUtils.ALL_MENU_TITLE);
            categories = categoriesList.ToArray();

            CategoryGroup.SetAllTogglesOff();
            CurrentCategories = categories;
            int count = CurrentCategories.Length;
            var categoryPrefab = Prefabs.CategoryTab;
            CategoryScroller.SpawnItems(categoryPrefab, count);
        }

        private void OnCategorySpawned(GameObject uiItem, int index)
        {
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
            if (category == UIUtils.ALL_MENU_TITLE)
                Section?.GetItems(OnGetItems);
            else
                Section?.GetItemsByCategory(category, OnGetItems);
        }

        // display items
        private void OnGetItems(List<CBSBaseItem> items)
        {
            var itemPrefab = Section.uiPrefab;
            ActiveScroller.Spawn(itemPrefab, items);
        }

        // trigger events
        public void CloseShop()
        {
            gameObject.SetActive(false);
        }

        public void OnTabChanged(bool val)
        {
            if (val)
            {
                var activeTab = ShopTabs.FirstOrDefault(x => x.isOn);
                if (activeTab != null)
                {
                    var tabTag = activeTab.GetComponent<ShopTab>();
                    var tab = tabTag.GetTab();
                    if (tab == ItemType.ITEMS)
                    {
                        Section = new ItemsSection();
                        ActiveScroller = ItemsScroller;
                    }
                    else if (tab == ItemType.PACKS)
                    {
                        Section = new PacksSection();
                        ActiveScroller = PacksScroller;
                    }
                    else if (tab == ItemType.LOOT_BOXES)
                    {
                        Section = new LootBoxSection();
                        ActiveScroller = LootBoxScroller;
                    }
                    DisplayCategories();
                }
            }
        }
    }
}

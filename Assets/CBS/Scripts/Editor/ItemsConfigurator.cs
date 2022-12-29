#if ENABLE_PLAYFABADMIN_API
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PlayFab;
using PlayFab.AdminModels;
using CBS.Scriptable;
using System.Linq;
using CBS.Editor.Window;
using CBS.Core;
using System;

namespace CBS.Editor
{
    public class ItemsConfigurator : BaseConfigurator
    {
        protected override string Title => "Items Configurator";

        private int SelectedToolBar { get; set; }

        public List<CatalogItem> Items { get; private set; }

        public List<CatalogItem> Packs { get; private set; }

        public List<CatalogItem> LootBoxes { get; private set; }

        public List<RandomResultTable> RandomItems { get; private set; }

        public Categories ItemsCategories { get; private set; }
        private Categories PacksCategories { get; set; }
        private Categories LootBoxesCategories { get; set; }
        private List<VirtualCurrencyData> VirtualCurrencies { get; set; }

        private GUILayoutOption[] AddButtonOptions
        {
            get
            {
                return new GUILayoutOption[] { GUILayout.Height(30), GUILayout.Width(140) };
            }
        }

        private bool ItemsInited { get; set; }

        private ItemsIcons Icons { get; set; }

        private int[] CategoryIndex { get; set; } = new int[] { 0, 0, 0 };

        private string SelectedCategory { get; set; }

        protected override bool DrawScrollView => false;

        private Rect CategoriesRect = new Rect(0,15,150,700);
        private Rect ItemsRect = new Rect(150, 115, 935, 700);
        private Rect RendomItemRect = new Rect(0, 115, 1085, 700);

        private Vector2 ItemsScroll { get; set; }
        private Vector2 PacksScroll { get; set; }
        private Vector2 LootBoxScroll { get; set; }
        private Vector2 RandomItemScroll { get; set; }

        private EditorData EditorData { get; set; }

        public override void Init(MenuTitles title)
        {
            base.Init(title);
            Icons = CBSScriptable.Get<ItemsIcons>();
            EditorData = CBSScriptable.Get<EditorData>();
            AllConfigurator.Add(this);
        }

        protected override void OnDrawInside()
        {
            // draw sub titles
            SelectedToolBar = GUILayout.Toolbar(SelectedToolBar, new string[] { "Items", "Packs", "Loot Boxes", "Randomize Items" });

            if (!ItemsInited)
            {
                InitConfigurator();
            }

            // check ready
            if (Items == null || ItemsCategories == null || VirtualCurrencies == null || RandomItems == null)
                return;

            switch (SelectedToolBar)
            {
                case 0:
                    DrawCategories(ItemsCategories, 0);
                    DrawItems();
                    break;
                case 1:
                    DrawCategories(PacksCategories, 1);
                    DrawPacks();
                    break;
                case 2:
                    DrawCategories(LootBoxesCategories, 2);
                    DrawLootboxes();
                    break;
                case 3:
                    DrawRandomizeItems();
                    break;
                default:
                    break;
            }
        }

        private void DrawCategories(Categories categories, int index)
        {
            using (var areaScope = new GUILayout.AreaScope(CategoriesRect))
            {
                var levelTitleStyle = new GUIStyle(GUI.skin.label);
                levelTitleStyle.fontStyle = FontStyle.Bold;
                levelTitleStyle.fontSize = 14;

                int catIndex = CategoryIndex[index];

                GUILayout.BeginVertical();

                GUILayout.Space(112);

                EditorGUILayout.LabelField("Categories", levelTitleStyle);

                int categoryHeight = 30;

                var categoriesMenu = categories.List.ToList();
                categoriesMenu.Remove(CBSConstants.UndefinedCategory);
                categoriesMenu.Insert(0, "All");
                catIndex = GUI.SelectionGrid(new Rect(0, 142, 150, categoryHeight * categoriesMenu.Count), catIndex, categoriesMenu.ToArray(), 1);
                string selctedCategory = categoriesMenu[catIndex];

                SelectedCategory = selctedCategory == "All" ? string.Empty : selctedCategory;

                GUILayout.Space(30);
                var oldColor = GUI.color;
                GUI.backgroundColor = EditorData.AddColor;
                var style = new GUIStyle(GUI.skin.button);
                style.fontStyle = FontStyle.Bold;
                style.fontSize = 12;
                if (GUI.Button(new Rect(0, 170 + categoryHeight * categoriesMenu.Count, 150, categoryHeight), "Add category", style))
                {
                    ModifyCateroriesWindow.Show(onModify => {
                        SaveCategories(onModify);
                    }, categories);
                    GUIUtility.ExitGUI();
                }
                GUI.backgroundColor = oldColor;

                GUILayout.EndVertical();

                CategoryIndex[index] = catIndex;
            }
        }

        private void DrawItems()
        {
            // draw titles
            using (var areaScope = new GUILayout.AreaScope(ItemsRect))
            {
                GUILayout.BeginHorizontal();
                // title style
                GUILayout.Space(20);
                var titleStyle = new GUIStyle(GUI.skin.label);
                titleStyle.alignment = TextAnchor.MiddleLeft;
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.fontSize = 14;
                // draw titles
                GUILayout.Label("ID", titleStyle, GUILayout.Width(100));
                GUILayout.Label("Sprite", titleStyle, GUILayout.Width(118));
                GUILayout.Label("Name", titleStyle, GUILayout.Width(150));
                GUILayout.Label("Count", titleStyle, GUILayout.Width(100));
                GUILayout.Label("Category", titleStyle, GUILayout.Width(100));
                GUILayout.Label("Price", titleStyle, GUILayout.Width(200));

                // add new item
                
                if (EditorUtils.DrawButton("Add new item", EditorData.AddColor, 12, AddButtonOptions))
                {
                    var currenciesList = VirtualCurrencies.Select(x => x.CurrencyCode).ToList();
                    AddItemWindow.Show<AddItemWindow>(new CatalogItem(), newItem => {
                        AddNewItem(newItem);
                    }, ItemAction.ADD, ItemsCategories.List, currenciesList, ItemType.ITEMS, CategoryIndex[0]);
                    GUIUtility.ExitGUI();
                }

                GUILayout.EndHorizontal();

                EditorUtils.DrawUILine(Color.grey, 2, 20);

                ItemsScroll = GUILayout.BeginScrollView(ItemsScroll);

                float cellHeight = 100;

                for (int i = 0; i < Items.Count; i++)
                {
                    var item = Items[i];

                    bool isConsumable = item.Consumable.UsageCount != null;

                    bool tagExist = item.Tags != null && item.Tags.Count != 0;
                    var category = tagExist ? item.Tags[0] : CBSConstants.UndefinedCategory;

                    if (!string.IsNullOrEmpty(SelectedCategory))
                    {
                        if (category != SelectedCategory)
                        {
                            continue;
                        }
                    }

                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    // title style
                    var levelTitleStyle = new GUIStyle(GUI.skin.label);
                    levelTitleStyle.fontStyle = FontStyle.Bold;
                    // draw id
                    EditorGUILayout.LabelField(item.ItemId, new GUILayoutOption[] { GUILayout.Width(100), GUILayout.Height(cellHeight) });
                    // draw icon
                    var actvieSprite = Icons.GetSprite(item.ItemId);
                    var iconTexture = actvieSprite == null ? null : actvieSprite.texture;
                    GUILayout.Button(iconTexture, GUILayout.Width(cellHeight), GUILayout.Height(cellHeight));
                    // draw display name
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField(item.DisplayName, new GUILayoutOption[] { GUILayout.Width(150), GUILayout.Height(cellHeight) });
                    // draw consumable option
                    string consumeOptions = isConsumable ? item.Consumable.UsageCount.ToString() : "Static";
                    EditorGUILayout.LabelField(consumeOptions, new GUILayoutOption[] { GUILayout.Width(100), GUILayout.Height(cellHeight) });
                    // draw caterory
                    EditorGUILayout.LabelField(category, new GUILayoutOption[] { GUILayout.Width(100), GUILayout.Height(cellHeight) });
                    // draw currencies
                    bool currenciesExist = item.VirtualCurrencyPrices != null && item.VirtualCurrencyPrices.Count > 0;
                    float currenciesHeight = currenciesExist ? cellHeight / item.VirtualCurrencyPrices.Count : cellHeight;
                    GUILayout.BeginVertical(GUILayout.Height(cellHeight));
                    if (currenciesExist)
                    {
                        GUILayout.FlexibleSpace();
                        var curList = item.VirtualCurrencyPrices;
                        foreach(var currency in curList)
                        {
                            GUILayout.BeginHorizontal(GUILayout.Width(200));
                            var curSprite = CBSScriptable.Get<CurrencyIcons>().GetSprite(currency.Key);
                            var curTexture = curSprite == null ? null : curSprite.texture;
                            GUILayout.Button(curTexture, GUILayout.Width(25), GUILayout.Height(25));
                            EditorGUILayout.LabelField(currency.Key, GUILayout.Width(25));
                            EditorGUILayout.LabelField(currency.Value.ToString(), GUILayout.Width(100));
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.FlexibleSpace();
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No prices", GUILayout.Width(200), GUILayout.Height(cellHeight));
                    }
                    
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical(GUILayout.Height(cellHeight));
                    GUILayout.FlexibleSpace();
                    // draw edit button
                    if (EditorUtils.DrawButton("Edit", EditorData.EditColor, 12, GUILayout.Width(75)))
                    {
                        var currenciesList = VirtualCurrencies.Select(x => x.CurrencyCode).ToList();
                        AddItemWindow.Show<AddItemWindow>(item, newItem => {
                            AddNewItem(newItem);
                        }, ItemAction.EDIT, ItemsCategories.List, currenciesList, ItemType.ITEMS, CategoryIndex[0]);
                        GUIUtility.ExitGUI();
                    }
                    // draw remove button
                    if (EditorUtils.DrawButton("Remove", EditorData.RemoveColor, 12, GUILayout.Width(75)))
                    {
                        int option = EditorUtility.DisplayDialogComplex("Warning",
                            "Are you sure you want to remove this item?",
                            "Yes",
                            "No",
                            string.Empty);
                        switch (option)
                        {
                            // ok.
                            case 0:
                                RemoveItem(item);
                                break;
                        }
                    }
                    // draw duplicate
                    if (EditorUtils.DrawButton("Duplicate", EditorData.DuplicateColor, 12, GUILayout.Width(75)))
                    {
                        EditorInputDialog.Show("Duplicate item ?", "Please enter new id", item.GetNextID(), OnDuplicate => {
                            var newId = OnDuplicate;
                            var newItem = item.Duplicate(newId);
                            AddNewItem(newItem);
                        });
                        GUIUtility.ExitGUI();
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();

                    GUILayout.EndHorizontal();

                    EditorUtils.DrawUILine(Color.grey, 1, 20);
                }

                GUILayout.Space(110);

                GUILayout.EndScrollView();
            }
        }

        private void DrawPacks()
        {
            GUILayout.Space(5);
            // tile style
            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 14;


            using (var areaScope = new GUILayout.AreaScope(ItemsRect))
            {
                GUILayout.BeginHorizontal();

                // draw titles
                GUILayout.Label("Pack", titleStyle, GUILayout.Width(230));
                GUILayout.Label("Items", titleStyle, GUILayout.Width(60));
                GUILayout.Label("Currencies", titleStyle, GUILayout.Width(500));

                GUILayout.FlexibleSpace();
                // add new item
                if (EditorUtils.DrawButton("Add new pack", EditorData.AddColor, 12, AddButtonOptions))
                {
                    var currenciesList = VirtualCurrencies.Select(x => x.CurrencyCode).ToList();
                    AddPackWindow.Show<AddPackWindow>(new CatalogItem(), newItem =>
                    {
                        AddNewItem(newItem);
                    }, ItemAction.ADD, PacksCategories.List, currenciesList, ItemType.PACKS, CategoryIndex[1]);
                    GUIUtility.ExitGUI();
                }
                GUILayout.EndHorizontal();

                EditorUtils.DrawUILine(Color.grey, 2, 20);

                GUILayout.Space(15);

                PacksScroll = GUILayout.BeginScrollView(PacksScroll);

                for (int i = 0; i < Packs.Count; i++)
                {
                    var pack = Packs[i];

                    bool tagExist = pack.Tags != null && pack.Tags.Count != 0;
                    var category = tagExist ? pack.Tags[0] : CBSConstants.UndefinedCategory;

                    if (!string.IsNullOrEmpty(SelectedCategory))
                    {
                        if (category != SelectedCategory)
                        {
                            continue;
                        }
                    }

                    GUILayout.BeginHorizontal();

                    GUILayout.Space(20);

                    GUILayout.BeginVertical();
                    // draw display name
                    EditorGUILayout.LabelField(pack.DisplayName, titleStyle, new GUILayoutOption[] { GUILayout.Width(200) });
                    // draw icon
                    var actvieSprite = Icons.GetSprite(pack.ItemId);
                    var iconTexture = actvieSprite == null ? null : actvieSprite.texture;
                    GUILayout.Button(iconTexture, GUILayout.Width(200), GUILayout.Height(200));
                    GUILayout.EndVertical();

                    // draw items
                    GUILayout.BeginVertical();
                    GUILayout.Space(20);
                    foreach (var item in pack.Bundle.BundledItems)
                    {
                        GUILayout.BeginHorizontal(GUILayout.Width(200));
                        GUILayout.Space(20);
                        var actvieItemSprite = Icons.GetSprite(item);
                        var itemTexture = actvieItemSprite == null ? null : actvieItemSprite.texture;
                        GUILayout.Button(itemTexture, GUILayout.Width(50), GUILayout.Height(50));
                        EditorGUILayout.LabelField(item);
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();

                    // draw currencies
                    GUILayout.BeginVertical();
                    GUILayout.Space(20);
                    if (pack.Bundle?.BundledVirtualCurrencies != null)
                    {
                        foreach (var currency in pack.Bundle.BundledVirtualCurrencies)
                        {
                            GUILayout.BeginHorizontal(GUILayout.Width(200));
                            GUILayout.Space(20);
                            var actvieCurrencySprite = CBSScriptable.Get<CurrencyIcons>().GetSprite(currency.Key);
                            var currencyTexture = actvieCurrencySprite == null ? null : actvieCurrencySprite.texture;
                            GUILayout.Button(currencyTexture, GUILayout.Width(50), GUILayout.Height(50));
                            EditorGUILayout.LabelField(currency.Key + " - " + currency.Value.ToString());
                            GUILayout.EndHorizontal();
                        }
                    }
                        
                    GUILayout.EndVertical();


                    GUILayout.FlexibleSpace();

                    GUILayout.BeginVertical();
                    GUILayout.Space(20);
                    // draw edit button
                    if (EditorUtils.DrawButton("Edit", EditorData.EditColor, 12, GUILayout.Width(75)))
                    {
                        var currenciesList = VirtualCurrencies.Select(x => x.CurrencyCode).ToList();
                        AddPackWindow.Show<AddPackWindow>(pack, newItem => {
                            AddNewItem(newItem);
                        }, ItemAction.EDIT, PacksCategories.List, currenciesList, ItemType.PACKS, CategoryIndex[1]);
                        GUIUtility.ExitGUI();
                    }
                    // draw remove button
                    if (EditorUtils.DrawButton("Remove", EditorData.RemoveColor, 12, GUILayout.Width(75)))
                    {
                        int option = EditorUtility.DisplayDialogComplex("Warning",
                            "Are you sure you want to this pack?",
                            "Yes",
                            "No",
                            string.Empty);
                        switch (option)
                        {
                            // ok.
                            case 0:
                                RemoveItem(pack);
                                break;
                        }
                    }
                    // draw dublicate
                    if (EditorUtils.DrawButton("Duplicate", EditorData.DuplicateColor, 12, GUILayout.Width(75)))
                    {
                        EditorInputDialog.Show("Duplicate item ?", "Please enter new id", pack.GetNextID(), OnDuplicate => {
                            var newId = OnDuplicate;
                            var newItem = pack.Duplicate(newId);
                            AddNewItem(newItem);
                        });
                        GUIUtility.ExitGUI();
                    }
                    GUILayout.EndVertical();

                    GUILayout.EndHorizontal();

                    EditorUtils.DrawUILine(Color.grey, 1, 20);

                    GUILayout.Space(20);
                }

                GUILayout.Space(110);

                GUILayout.EndScrollView();
            }
        }

        private void DrawLootboxes()
        {
            GUILayout.Space(5);
            // tile style
            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 14;


            using (var areaScope = new GUILayout.AreaScope(ItemsRect))
            {
                GUILayout.BeginHorizontal();

                // draw titles
                GUILayout.Label("Lootbox", titleStyle, GUILayout.Width(230));
                GUILayout.Label("Items", titleStyle, GUILayout.Width(60));
                GUILayout.Label("Currencies", titleStyle, GUILayout.Width(500));

                GUILayout.FlexibleSpace();
                // add new item
                if (EditorUtils.DrawButton("Add new Lootbox", EditorData.AddColor, 12, AddButtonOptions))
                {
                    var currenciesList = VirtualCurrencies.Select(x => x.CurrencyCode).ToList();
                    AddLootBoxWindow.Show<AddLootBoxWindow>(new CatalogItem(), newItem =>
                    {
                        AddNewItem(newItem);
                    }, ItemAction.ADD, LootBoxesCategories.List, currenciesList, ItemType.LOOT_BOXES, CategoryIndex[2]);
                    GUIUtility.ExitGUI();
                }
                GUILayout.EndHorizontal();

                EditorUtils.DrawUILine(Color.grey, 2, 20);

                GUILayout.Space(15);

                LootBoxScroll = GUILayout.BeginScrollView(LootBoxScroll);

                for (int i = 0; i < LootBoxes.Count; i++)
                {
                    var box = LootBoxes[i];

                    bool tagExist = box.Tags != null && box.Tags.Count != 0;
                    var category = tagExist ? box.Tags[0] : CBSConstants.UndefinedCategory;

                    if (!string.IsNullOrEmpty(SelectedCategory))
                    {
                        if (category != SelectedCategory)
                        {
                            continue;
                        }
                    }

                    GUILayout.BeginHorizontal();

                    GUILayout.Space(20);

                    GUILayout.BeginVertical();
                    // draw display name
                    EditorGUILayout.LabelField(box.DisplayName, titleStyle, new GUILayoutOption[] { GUILayout.Width(200) });
                    // draw icon
                    var actvieSprite = Icons.GetSprite(box.ItemId);
                    var iconTexture = actvieSprite == null ? null : actvieSprite.texture;
                    GUILayout.Button(iconTexture, GUILayout.Width(200), GUILayout.Height(200));
                    GUILayout.EndVertical();

                    // draw items
                    GUILayout.BeginVertical();
                    GUILayout.Space(20);
                    foreach (var item in box.Container.ResultTableContents)
                    {
                        GUILayout.BeginHorizontal(GUILayout.Width(200));
                        GUILayout.Space(20);
                        var actvieItemSprite = Icons.GetSprite(item);
                        var itemTexture = actvieItemSprite == null ? null : actvieItemSprite.texture;
                        GUILayout.Button(itemTexture, GUILayout.Width(50), GUILayout.Height(50));
                        EditorGUILayout.LabelField(item);
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();

                    // draw currencies
                    GUILayout.BeginVertical();
                    GUILayout.Space(20);
                    if (box.Container?.VirtualCurrencyContents != null)
                    {
                        foreach (var currency in box.Container.VirtualCurrencyContents)
                        {
                            GUILayout.BeginHorizontal(GUILayout.Width(200));
                            GUILayout.Space(20);
                            var actvieCurrencySprite = CBSScriptable.Get<CurrencyIcons>().GetSprite(currency.Key);
                            var currencyTexture = actvieCurrencySprite == null ? null : actvieCurrencySprite.texture;
                            GUILayout.Button(currencyTexture, GUILayout.Width(50), GUILayout.Height(50));
                            EditorGUILayout.LabelField(currency.Key + " - " + currency.Value.ToString());
                            GUILayout.EndHorizontal();
                        }
                    }
                        
                    GUILayout.EndVertical();

                    GUILayout.FlexibleSpace();

                    GUILayout.BeginVertical();
                    GUILayout.Space(20);
                    // draw edit button
                    if (EditorUtils.DrawButton("Edit", EditorData.EditColor, 12, GUILayout.Width(75)))
                    {
                        var currenciesList = VirtualCurrencies.Select(x => x.CurrencyCode).ToList();
                        AddLootBoxWindow.Show<AddLootBoxWindow>(box, newItem => {
                            AddNewItem(newItem);
                        }, ItemAction.EDIT, LootBoxesCategories.List, currenciesList, ItemType.LOOT_BOXES, CategoryIndex[2]);
                        GUIUtility.ExitGUI();
                    }
                    // draw remove button
                    if (EditorUtils.DrawButton("Remove", EditorData.RemoveColor, 12, GUILayout.Width(75)))
                    {
                        int option = EditorUtility.DisplayDialogComplex("Warning",
                            "Are you sure you want to remove this pack?",
                            "Yes",
                            "No",
                            string.Empty);
                        switch (option)
                        {
                            // ok.
                            case 0:
                                RemoveItem(box);
                                break;
                        }
                    }
                    // draw dublicate
                    if (EditorUtils.DrawButton("Duplicate", EditorData.DuplicateColor, 12, GUILayout.Width(75)))
                    {
                        EditorInputDialog.Show("Duplicate item ?", "Please enter new id", box.GetNextID(), OnDuplicate => {
                            var newId = OnDuplicate;
                            var newItem = box.Duplicate(newId);
                            AddNewItem(newItem);
                        });
                        GUIUtility.ExitGUI();
                    }
                    GUILayout.EndVertical();

                    GUILayout.EndHorizontal();

                    EditorUtils.DrawUILine(Color.grey, 1, 20);

                    GUILayout.Space(20);
                }

                GUILayout.Space(110);

                GUILayout.EndScrollView();
            }
        }

        private void DrawRandomizeItems()
        {
            GUILayout.Space(5);
            // tile style
            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.alignment = TextAnchor.MiddleLeft;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 14;


            using (var areaScope = new GUILayout.AreaScope(RendomItemRect))
            {
                GUILayout.BeginHorizontal();

                // draw titles
                GUILayout.Label("Random Item ID", titleStyle, GUILayout.Width(500));
                GUILayout.Label("Content", titleStyle, GUILayout.Width(60));

                GUILayout.FlexibleSpace();
                // add new item
                if (EditorUtils.DrawButton("Add new random item", EditorData.AddColor, 12, GUILayout.Width(150), GUILayout.Height(30)))
                {
                    var currenciesList = VirtualCurrencies.Select(x => x.CurrencyCode).ToList();
                    AddRandomItemWindow.Show(newItem =>
                    {
                        if (RandomItems != null)
                        {
                            RandomItems.Add(newItem);
                            SaveRandomItems(RandomItems);
                        }
                    }, new RandomResultTable());
                    GUIUtility.ExitGUI();
                }
                GUILayout.EndHorizontal();

                EditorUtils.DrawUILine(Color.grey, 2, 20);

                GUILayout.Space(15);

                RandomItemScroll = GUILayout.BeginScrollView(RandomItemScroll);

                float rowWith = 900;
                int rowCount = 9;

                for (int i = 0; i < RandomItems.Count; i++)
                {
                    GUILayout.BeginHorizontal(GUILayout.Width(rowWith + 150));

                    var randomItem = RandomItems[i];
                    string itemID = randomItem.TableId;

                    GUILayout.BeginVertical();
                    // draw display name
                    EditorGUILayout.LabelField(itemID, new GUILayoutOption[] { GUILayout.Width(100) });
                    // draw icon
                    var actvieSprite = Icons.GetSprite(itemID);
                    var iconTexture = actvieSprite == null ? null : actvieSprite.texture;
                    GUILayout.Button(iconTexture, GUILayout.Width(100), GUILayout.Height(100));
                    GUILayout.EndVertical();

                    GUILayout.Space(10);

                    int allWeight = randomItem.Nodes.Select(x => x.Weight).Sum();
                    
                    // draw nodes
                    for (int j = 0; j < randomItem.Nodes.Count; j++)
                    {
                        if (j% rowCount == 0 && j != 0)
                        {
                            if (j <= rowCount)
                            {
                                DrawRandomEditRemove(randomItem);
                            }

                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal(GUILayout.Width(rowWith));

                            if (i != rowCount)
                            {
                                GUILayout.Space(115);
                            }
                        }

                        GUILayout.Space(10);

                        GUILayout.BeginVertical();

                        GUILayout.Space(22);

                        var node = randomItem.Nodes[j];
                        string nodeId = node.ResultItem;
                        int weight = node.Weight;

                        float persent = (float)weight / (float)allWeight;

                        var nodeSprite = Icons.GetSprite(nodeId);
                        var nodeTexture = nodeSprite == null ? null : nodeSprite.texture;
                        GUILayout.Button(nodeTexture, GUILayout.Width(75), GUILayout.Height(75));

                        float lastY = GUILayoutUtility.GetLastRect().y;
                        float lastX = GUILayoutUtility.GetLastRect().x;

                        string progressTitle = (persent * 100).ToString("0.00") + "%";

                        EditorGUI.ProgressBar(new Rect(lastX, lastY + 80, 75, 20), persent, progressTitle);

                        GUILayout.Space(25);

                        GUILayout.EndVertical();
                    }

                    GUILayout.FlexibleSpace();

                    if (randomItem.Nodes.Count <= rowCount)
                    {
                        //GUILayout.Space(100);
                        DrawRandomEditRemove(randomItem);
                    }

                    GUILayout.EndHorizontal();

                    EditorUtils.DrawUILine(Color.grey, 1, 20);

                    GUILayout.Space(30);
                }


                GUILayout.Space(110);

                GUILayout.EndScrollView();
            }
        }

        private void DrawRandomEditRemove(RandomResultTable randomItem)
        {
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            GUILayout.Space(20);
            // draw edit button
            if (EditorUtils.DrawButton("Edit", EditorData.EditColor, 12, GUILayout.Width(75)))
            {
                AddRandomItemWindow.Show(newItem =>
                {
                    if (RandomItems != null)
                    {
                        SaveRandomItems(RandomItems);
                    }
                }, randomItem);
                GUIUtility.ExitGUI();
            }
            // draw remove button
            if (EditorUtils.DrawButton("Remove", EditorData.RemoveColor, 12, GUILayout.Width(75)))
            {
                int option = EditorUtility.DisplayDialogComplex("Warning",
                    "Are you sure you want to remove this item?",
                    "Yes",
                    "No",
                    string.Empty);
                switch (option)
                {
                    // ok.
                    case 0:
                        if (RandomItems.Contains(randomItem))
                        {
                            RandomItems.Remove(randomItem);
                            RandomItems.TrimExcess();
                        }
                        SaveRandomItems(RandomItems);
                        break;
                }
            }
            GUILayout.EndVertical();
        }

        private void InitConfigurator()
        {
            // get categories
            GetCategories(categotyResult => {
                OnCategiriesGetted(categotyResult);
                // get items
                GetItemsCatalog(itemsResult => {
                    OnItemsCatalogGetted(itemsResult);
                    // currencies
                    GetAllCurrencies(currenciesResult => {
                        OnCurrenciesListGetted(currenciesResult);
                        // random items
                        GetRandomItems(randomItemsResult => {
                            OnGetRandomTables(randomItemsResult);
                        });
                    });
                });
            });
            
            ItemsInited = true;
        }

        // fab methods

        // get random items
        private void GetRandomItems(Action<GetRandomResultTablesResult> finish = null)
        {
            ShowProgress();

            var dataRequest = new GetRandomResultTablesRequest
            {
                CatalogVersion = CBSConstants.ItemsCatalogID
            };

            var succesCallback = finish == null ? OnGetRandomTables : finish;

            PlayFabAdminAPI.GetRandomResultTables(dataRequest, succesCallback, OnGetRandomTablesFailed);
        }

        private void OnGetRandomTables(GetRandomResultTablesResult result)
        {
            RandomItems = new List<RandomResultTable>();

            foreach (var table in result.Tables)
            {
                RandomItems.Add(new RandomResultTable { 
                    TableId = table.Value.TableId,
                    Nodes = table.Value.Nodes
                });
            }
            HideProgress();
        }

        private void OnGetRandomTablesFailed(PlayFabError error)
        {
            HideProgress();
            Debug.Log(error.Error);
            AddErrorLog(error);
        }

        // save random items
        private void SaveRandomItems(List<RandomResultTable> newList)
        {
            ShowProgress();

            var dataRequest = new UpdateRandomResultTablesRequest { 
                CatalogVersion = CBSConstants.ItemsCatalogID,
                Tables = newList
            };

            PlayFabAdminAPI.UpdateRandomResultTables(dataRequest, OnRandomTablesUpdated, OnUpdateRandomTablesFailed);
        }

        private void OnRandomTablesUpdated(UpdateRandomResultTablesResult result)
        {
            Debug.Log("OnRandomTablesUpdated");
            HideProgress();
            GetRandomItems();
        }

        private void OnUpdateRandomTablesFailed(PlayFabError error)
        {
            HideProgress();
            Debug.Log(error.Error);
            AddErrorLog(error);
        }

        // get categories
        public void GetCategories(Action<GetTitleDataResult> finish = null)
        {
            ShowProgress();

            var dataRequest = new GetTitleDataRequest
            {
                Keys = new List<string> { 
                    CBSConstants.CategoriesKey,
                    CBSConstants.PackCategoriesKey,
                    CBSConstants.LootboxesCategoriesKey
                }
            };

            var succesCallback = finish == null ? OnCategiriesGetted : finish;

            PlayFabAdminAPI.GetTitleData(dataRequest, succesCallback, OnGetTitleDataFailed);
        }

        private void OnCategiriesGetted(GetTitleDataResult result)
        {
            // items categories
            if (result.Data.ContainsKey(CBSConstants.CategoriesKey))
            {
                var rawData = result.Data[CBSConstants.CategoriesKey];
                ItemsCategories = JsonUtility.FromJson<Categories>(rawData);
                if (!ItemsCategories.List.Contains(CBSConstants.UndefinedCategory))
                {
                    ItemsCategories.List.Insert(0, CBSConstants.UndefinedCategory);
                }
                ItemsCategories.TitleKey = CBSConstants.CategoriesKey;
            }
            else
            {
                ItemsCategories = new Categories();
                ItemsCategories.List = new List<string>();
                ItemsCategories.List.Add(CBSConstants.UndefinedCategory);
                ItemsCategories.TitleKey = CBSConstants.CategoriesKey;
            }
            // packs categories
            if (result.Data.ContainsKey(CBSConstants.PackCategoriesKey))
            {
                var rawData = result.Data[CBSConstants.PackCategoriesKey];
                PacksCategories = JsonUtility.FromJson<Categories>(rawData);
                if (!PacksCategories.List.Contains(CBSConstants.UndefinedCategory))
                {
                    PacksCategories.List.Insert(0, CBSConstants.UndefinedCategory);
                }
                PacksCategories.TitleKey = CBSConstants.PackCategoriesKey;
            }
            else
            {
                PacksCategories = new Categories();
                PacksCategories.List = new List<string>();
                PacksCategories.List.Add(CBSConstants.UndefinedCategory);
                PacksCategories.TitleKey = CBSConstants.PackCategoriesKey;
            }
            // loot boxes categories
            if (result.Data.ContainsKey(CBSConstants.LootboxesCategoriesKey))
            {
                var rawData = result.Data[CBSConstants.LootboxesCategoriesKey];
                LootBoxesCategories = JsonUtility.FromJson<Categories>(rawData);
                if (!LootBoxesCategories.List.Contains(CBSConstants.UndefinedCategory))
                {
                    LootBoxesCategories.List.Insert(0, CBSConstants.UndefinedCategory);
                }
                LootBoxesCategories.TitleKey = CBSConstants.LootboxesCategoriesKey;
            }
            else
            {
                LootBoxesCategories = new Categories();
                LootBoxesCategories.List = new List<string>();
                LootBoxesCategories.List.Add(CBSConstants.UndefinedCategory);
                LootBoxesCategories.TitleKey = CBSConstants.LootboxesCategoriesKey;
            }
            HideProgress();
        }

        private void OnGetTitleDataFailed(PlayFabError error)
        {
            HideProgress();
            Debug.Log(error.Error);
            AddErrorLog(error);
        }

        // save category
        private void SaveCategories(Categories categories)
        {
            ShowProgress();

            var list = categories.List.ToList();
            if (list.Contains(CBSConstants.UndefinedCategory))
            {
                list.Remove(CBSConstants.UndefinedCategory);
            }

            categories.List = list;

            string rawData = JsonUtility.ToJson(categories);

            var dataRequest = new SetTitleDataRequest {
                Key = categories.TitleKey,
                Value = rawData
            };

            PlayFabAdminAPI.SetTitleData(dataRequest, OnCategoriesSaved, OnSaveCategoriesFailed);
        }

        private void OnCategoriesSaved(SetTitleDataResult result)
        {
            HideProgress();
            GetCategories();
        }

        private void OnSaveCategoriesFailed(PlayFabError error)
        {
            HideProgress();
            Debug.Log(error.Error);
            AddErrorLog(error);
        }

        // add new item
        private void AddNewItem(CatalogItem item)
        {
            Items.Add(item);
            SaveCatalog();
        }

        // remove item
        private void RemoveItem(CatalogItem pack)
        {
            if (Items.Contains(pack))
            {
                Icons.RemoveSprite(pack.ItemId);
                Items.Remove(pack);
                Items.TrimExcess();
            }
            if (Packs.Contains(pack))
            {
                Icons.RemoveSprite(pack.ItemId);
                Packs.Remove(pack);
                Packs.TrimExcess();
            }
            if (LootBoxes.Contains(pack))
            {
                Icons.RemoveSprite(pack.ItemId);
                LootBoxes.Remove(pack);
                LootBoxes.TrimExcess();
            }
            OverrideCatalog();
        }

        // override catalog
        private void OverrideCatalog()
        {
            ShowProgress();

            var allItems = Items.Concat(Packs).ToList();
            allItems = allItems.Concat(LootBoxes).ToList();

            var dataRequest = new UpdateCatalogItemsRequest
            {
                Catalog = allItems,
                CatalogVersion = CBSConstants.ItemsCatalogID
            };

            PlayFabAdminAPI.SetCatalogItems(dataRequest, OnCatalogUpdated, OnCatalogUpdatedFailed);
        }

        // save catalog
        private void SaveCatalog()
        {
            ShowProgress();

            var dataRequest = new UpdateCatalogItemsRequest {
                Catalog = Items,
                CatalogVersion = CBSConstants.ItemsCatalogID,
                SetAsDefaultCatalog = true
            };

            PlayFabAdminAPI.UpdateCatalogItems(dataRequest, OnCatalogUpdated, OnCatalogUpdatedFailed);
        }

        private void OnCatalogUpdated(UpdateCatalogItemsResult result)
        {
            HideProgress();

            GetItemsCatalog();
        }

        private void OnCatalogUpdatedFailed(PlayFabError error)
        {
            HideProgress();
            Debug.Log(error.Error);
            AddErrorLog(error);
        }

        // get items
        public void GetItemsCatalog(Action<GetCatalogItemsResult> finish = null)
        {
            ShowProgress();
            var dataRequest = new GetCatalogItemsRequest
            {
                CatalogVersion = CBSConstants.ItemsCatalogID
            };

            var succesCallback = finish == null ? OnItemsCatalogGetted : finish;

            PlayFabAdminAPI.GetCatalogItems(dataRequest, succesCallback, OnGetCatalogFailed);
        }

        private void OnItemsCatalogGetted(GetCatalogItemsResult result)
        {
            HideProgress();
            Items = result.Catalog.Where(x=>x.Bundle == null && x.Container == null).ToList();
            Packs = result.Catalog.Where(x => x.Bundle != null).ToList();
            LootBoxes = result.Catalog.Where(x => x.Container != null).ToList();
        }

        private void OnGetCatalogFailed(PlayFabError error)
        {
            HideProgress();
            Debug.Log(error.Error);
            AddErrorLog(error);
        }

        // get currency
        public void GetAllCurrencies(Action<ListVirtualCurrencyTypesResult> finish = null)
        {
            ShowProgress();

            var dataRequest = new ListVirtualCurrencyTypesRequest();

            var succesCallback = finish == null ? OnCurrenciesListGetted : finish;

            PlayFabAdminAPI.ListVirtualCurrencyTypes(dataRequest, succesCallback, OnGetCurrenciesListFailed);
        }

        private void OnCurrenciesListGetted(ListVirtualCurrencyTypesResult result)
        {
            Debug.Log("OnCurrenciesListGetted");
            HideProgress();
            VirtualCurrencies = result.VirtualCurrencies;
        }

        private void OnGetCurrenciesListFailed(PlayFabError error)
        {
            HideProgress();
            AddErrorLog(error);
        }
    }

    public enum ItemAction
    {
        ADD,
        EDIT
    }
}
#endif

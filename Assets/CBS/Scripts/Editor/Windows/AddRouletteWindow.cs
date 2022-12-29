#if ENABLE_PLAYFABADMIN_API
using CBS.Core;
using CBS.Scriptable;
using PlayFab.AdminModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CBS.Editor.Window
{
    public class AddRouletteWindow : EditorWindow
    {
        private static List<CatalogItem> AllItems = new List<CatalogItem>();
        private static List<CatalogItem> AllLootboxes = new List<CatalogItem>();
        private static Categories ItemCategories = new Categories();
        private static Categories LootboxCategories = new Categories();

        private static ItemAction Action { get; set; }
        private static Action<RoulettePosition> ModifyCallback { get; set; }
        private static bool IncludeCurrencies { get; set; }

        private static PrizeObject Bundle { get; set; }
        private static RoulettePosition RoulettePosition { get; set; }

        private string SelectedItemID { get; set; }
        private int SelectedItemIndex { get; set; }
        private string SelectedLootID { get; set; }
        private int SelectedLootIndex { get; set; }
        private string ID { get; set; }
        private string DisplayName { get; set; }
        private int Weight { get; set; }

        private int SelectedItemCatagoryID { get; set; } = -1;
        private int SelectedLootCatagoryID { get; set; } = -1;
        private int SelectedCurrencyIndex { get; set; }

        private string[] AllItemsIDs { get; set; }
        private string[] AllLootboxIDs { get; set; }

        private ItemsIcons Icons { get; set; }
        private CurrencyIcons CurrenciesIcons { get; set; }

        private Sprite IconSprite { get; set; }

        private static List<string> Currencies { get; set; }

        private Vector2 ScrollPos { get; set; }

        public static void Show(RouletteWindowRequest request)
        {
            AllItems = request.items.Where(x => x.Bundle == null && x.Container == null).ToList();
            AllLootboxes = request.items.Where(x => x.Container != null).ToList();
            ItemCategories = request.itemCategories;
            LootboxCategories = request.lootboxCategories;
            Currencies = request.currencies;
            IncludeCurrencies = request.includeCurencies;
            RoulettePosition = request.position ?? new RoulettePosition();
            Bundle = request.position.Prize ?? new PrizeObject();
            Bundle.BundledItems = Bundle.BundledItems ?? new List<string>();
            Bundle.Lootboxes = Bundle.Lootboxes ?? new List<string>();
            Bundle.BundledVirtualCurrencies = Bundle.BundledVirtualCurrencies ?? new Dictionary<string, uint>();
            ModifyCallback = request.modifyCallback;
            Action = request.Action;

            AddRouletteWindow window = ScriptableObject.CreateInstance<AddRouletteWindow>();
            window.maxSize = new Vector2(400, 700);
            window.minSize = window.maxSize;
            window.Init();
            window.ShowUtility();
            
        }

        private void Hide()
        {
            this.Close();
        }

        public void Init()
        {
            Icons = CBSScriptable.Get<ItemsIcons>();
            CurrenciesIcons = CBSScriptable.Get<CurrencyIcons>();

            ID = RoulettePosition.ID;
            DisplayName = RoulettePosition.DisplayName;
            Weight = RoulettePosition.Weight;
            IconSprite = Icons.GetSprite(ID);
        }

        void OnGUI()
        {
            using (var areaScope = new GUILayout.AreaScope(new Rect(0, 0, 400, 700)))
            {
                ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);
                GUILayout.Space(15);
                if (Bundle == null)
                    return;

                var titleStyle = new GUIStyle(GUI.skin.label);
                titleStyle.alignment = TextAnchor.MiddleCenter;
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.fontSize = 14;

                // draw items

                if (Action == ItemAction.ADD)
                {
                    ID = EditorGUILayout.TextField("Roulette position ID", ID);
                }
                if (Action == ItemAction.EDIT)
                {
                    EditorGUILayout.LabelField("Roulette position ID", ID);
                }
                EditorGUILayout.HelpBox("Unique id for roulette position.", MessageType.Info);

                GUILayout.Space(15);
                DisplayName = EditorGUILayout.TextField("Display name", DisplayName);
                EditorGUILayout.HelpBox("Display name of the roulette item", MessageType.Info);

                GUILayout.Space(15);
                Weight = EditorGUILayout.IntField("Weight", Weight);
                EditorGUILayout.HelpBox("The greater the weight, the greater the chance that this product will dropped.", MessageType.Info);

                GUILayout.Space(15);
                // draw icon
                EditorGUILayout.LabelField("Sprite", new GUILayoutOption[] { GUILayout.MaxWidth(150) });
                IconSprite = (Sprite)EditorGUILayout.ObjectField((IconSprite as UnityEngine.Object), typeof(Sprite), false, new GUILayoutOption[] { GUILayout.MaxWidth(150) });
                EditorGUILayout.HelpBox("Sprite for roulete item. ATTENTION! The sprite is not saved on the server, it will be included in the build", MessageType.Info);


                // draw prizes

                GUILayout.Space(25);

                EditorGUILayout.LabelField("Items", titleStyle);

                GUILayout.BeginVertical();

                GUILayout.Space(15);

                for (int i = 0; i < Bundle.BundledItems.Count; i++)
                {
                    string key = Bundle.BundledItems[i];

                    if (key == CBSConstants.UndefinedCategory)
                        continue;

                    GUILayout.BeginHorizontal();

                    GUILayout.Space(15);

                    var actvieSprite = Icons.GetSprite(key);
                    var iconTexture = actvieSprite == null ? null : actvieSprite.texture;
                    GUILayout.Button(iconTexture, GUILayout.Width(50), GUILayout.Height(50));

                    EditorGUILayout.LabelField(key);

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        Bundle.BundledItems.Remove(key);
                        Bundle.BundledItems.TrimExcess();
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(15);

                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Category");
                GUILayout.Label("Item ID");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                var itemList = ItemCategories.List ?? new List<string>();
                int selectedCategory = EditorGUILayout.Popup(SelectedItemCatagoryID == -1 ? 0 : SelectedItemCatagoryID, itemList.ToArray());
                if (selectedCategory != SelectedItemCatagoryID)
                {
                    if (itemList.Count > 0)
                    {
                        Debug.Log("On category selected");
                        string category = itemList[selectedCategory];
                        AllItemsIDs = AllItems.Where(i => i.Tags[0] == category).Select(x => x.ItemId).ToArray();
                    }
                    else
                    {
                        AllItemsIDs = AllItems.Select(x => x.ItemId).ToArray();
                    }
                }
                SelectedItemCatagoryID = selectedCategory;
                SelectedItemIndex = EditorGUILayout.Popup(SelectedItemIndex, AllItemsIDs);
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                GUILayout.EndVertical();

                if (GUILayout.Button("Add"))
                {
                    if (AllItemsIDs.Length > 0)
                    {
                        SelectedItemID = AllItemsIDs[SelectedItemIndex];
                        if (!string.IsNullOrEmpty(SelectedItemID))
                        {
                            Bundle.BundledItems.Add(SelectedItemID);
                        }
                    }
                }

                GUILayout.EndVertical();

                GUILayout.Space(30);

                // draw loot boxes

                EditorGUILayout.LabelField("Loot boxes", titleStyle);

                GUILayout.BeginVertical();

                GUILayout.Space(15);

                for (int i = 0; i < Bundle.Lootboxes.Count; i++)
                {
                    string key = Bundle.Lootboxes[i];

                    if (key == CBSConstants.UndefinedCategory)
                        continue;

                    GUILayout.BeginHorizontal();

                    GUILayout.Space(15);

                    var actvieSprite = Icons.GetSprite(key);
                    var iconTexture = actvieSprite == null ? null : actvieSprite.texture;
                    GUILayout.Button(iconTexture, GUILayout.Width(50), GUILayout.Height(50));

                    EditorGUILayout.LabelField(key);

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        Bundle.Lootboxes.Remove(key);
                        Bundle.Lootboxes.TrimExcess();
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(15);

                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Category");
                GUILayout.Label("Lootbox ID");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                var lootBoxList = LootboxCategories.List ?? new List<string>();
                int selectedLootCategory = EditorGUILayout.Popup(SelectedLootCatagoryID == -1 ? 0 : SelectedLootCatagoryID, lootBoxList.ToArray());
                if (selectedLootCategory != SelectedLootCatagoryID)
                {
                    Debug.Log("On category selected");
                    if (lootBoxList.Count > 0)
                    {
                        string category = lootBoxList[selectedLootCategory];
                        AllLootboxIDs = AllLootboxes.Where(i => i.Tags[0] == category).Select(x => x.ItemId).ToArray();
                    }
                    else
                    {
                        AllLootboxIDs = AllLootboxes.Select(x => x.ItemId).ToArray();
                    }
                }
                SelectedLootCatagoryID = selectedLootCategory;
                SelectedLootIndex = EditorGUILayout.Popup(SelectedLootIndex, AllLootboxIDs);
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                GUILayout.EndVertical();

                if (GUILayout.Button("Add"))
                {
                    if (AllLootboxIDs.Length > 0)
                    {
                        SelectedLootID = AllLootboxIDs[SelectedLootIndex];
                        if (!string.IsNullOrEmpty(SelectedLootID))
                        {
                            Bundle.Lootboxes.Add(SelectedLootID);
                        }
                    }
                }

                GUILayout.EndVertical();

                // draw currencies
                if (Currencies != null && Currencies.Count != 0 && IncludeCurrencies)
                {
                    // draw currencies list
                    GUILayout.Space(30);
                    EditorGUILayout.LabelField("Currencies", titleStyle);

                    GUILayout.Space(15);

                    if (Bundle.BundledVirtualCurrencies != null && Bundle.BundledVirtualCurrencies.Count != 0)
                    {
                        for (int i = 0; i < Bundle.BundledVirtualCurrencies.Count; i++)
                        {
                            string key = Bundle.BundledVirtualCurrencies.Keys.ElementAt(i);
                            int val = (int)Bundle.BundledVirtualCurrencies.Values.ElementAt(i);
                            GUILayout.BeginHorizontal();

                            GUILayout.Space(15);

                            var actvieSprite = CurrenciesIcons.GetSprite(key);
                            var iconTexture = actvieSprite == null ? null : actvieSprite.texture;
                            GUILayout.Button(iconTexture, GUILayout.Width(50), GUILayout.Height(50));

                            val = EditorGUILayout.IntField(key, val);

                            Bundle.BundledVirtualCurrencies[key] = (uint)val;

                            if (GUILayout.Button("X", GUILayout.Width(20)))
                            {
                                Bundle.BundledVirtualCurrencies.Remove(key);
                            }

                            GUILayout.EndHorizontal();
                        }
                    }
                    // add currency button
                    GUILayout.Space(15);
                    GUILayout.BeginHorizontal();
                    if (Currencies != null && Currencies.Count != 0)
                    {
                        SelectedCurrencyIndex = EditorGUILayout.Popup(SelectedCurrencyIndex, Currencies.ToArray());
                        string defaultKey = Currencies[SelectedCurrencyIndex];
                        if (GUILayout.Button("Add currency"))
                        {
                            if (!Bundle.BundledVirtualCurrencies.ContainsKey(defaultKey))
                                Bundle.BundledVirtualCurrencies[defaultKey] = 0;
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(30);

                EditorGUILayout.EndScrollView();

                GUILayout.FlexibleSpace();

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Save"))
                {
                    ApplyChanges(RoulettePosition);
                    if (IsValidInput())
                    {
                        ModifyCallback?.Invoke(RoulettePosition);
                        Hide();
                    }
                    else
                    {
                        Debug.Log("Not valid input");
                    }
                }
                if (GUILayout.Button("Close"))
                {
                    Hide();
                }

                GUILayout.EndHorizontal();
            }
        }

        private void ApplyChanges(RoulettePosition position)
        {
            position.ID = ID;
            position.DisplayName = DisplayName;
            position.Prize = Bundle;
            position.Weight = Weight;

            if (IconSprite == null)
            {
                Icons.RemoveSprite(ID);
            }
            else
            {
                Icons.SaveSprite(ID, IconSprite);
            }
        }

        private bool IsValidInput()
        {
            if (string.IsNullOrEmpty(ID))
                return false;
            if (RoulettePosition.Prize == null || RoulettePosition.Prize.IsEmpty())
                return false;
            return true;
        }
    }

    public struct RouletteWindowRequest
    {
        public Action<RoulettePosition> modifyCallback;
        public List<CatalogItem> items;
        public Categories itemCategories;
        public Categories lootboxCategories;
        public List<string> currencies;
        public RoulettePosition position;
        public bool includeCurencies;
        public ItemAction Action;
    }
}
#endif
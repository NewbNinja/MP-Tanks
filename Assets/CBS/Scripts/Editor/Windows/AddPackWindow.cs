#if ENABLE_PLAYFABADMIN_API
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.AdminModels;
using UnityEditor;
using System.Linq;
using CBS.Scriptable;
using CBS.Core;

namespace CBS.Editor.Window
{
    public class AddPackWindow : AddItemWindow
    {
        private CatalogItemBundleInfo Bundle { get; set; }

        private ItemsConfigurator ItemsConfigurator { get; set; }

        private string SelectedItemID { get; set; }
        private int SelectedItemIndex { get; set; }

        private int SelectedItemCatagoryID { get; set; } = -1;
        private int SelectedCurrencyIndex { get; set; }

        private string [] AllItemsIDs { get; set; }
        private Categories ItemCategories { get; set; }

        private ItemsIcons Icons { get; set; }
        private CurrencyIcons CurrenciesIcons { get; set; }

        protected override void Init()
        {
            base.Init();

            Icons = CBSScriptable.Get<ItemsIcons>();
            CurrenciesIcons = CBSScriptable.Get<CurrencyIcons>();
            ItemsConfigurator = BaseConfigurator.Get<ItemsConfigurator>();
            AllItemsIDs = ItemsConfigurator.Items.Select(x => x.ItemId).ToArray();
            Debug.Log("Items count = " + AllItemsIDs.Length);
            ItemCategories = ItemsConfigurator.ItemsCategories;

            Titles[1] = "Packs";
            AddTitle = "Add Pack";
            SaveTitle = "Save Pack";

            Bundle = CurrentData.Bundle ?? new CatalogItemBundleInfo();
            if (Bundle.BundledItems == null)
            {
                Bundle.BundledItems = new List<string>();
            }
            if (Bundle.BundledVirtualCurrencies == null)
            {
                Bundle.BundledVirtualCurrencies = new Dictionary<string, uint>();
            }
        }

        protected override void CheckInputs()
        {
            base.CheckInputs();

            CurrentData.Bundle = Bundle;
        }

        protected override void DrawConfigs()
        {
            GUILayout.Space(15);
            if (Bundle == null)
                return;

            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 14;

            // draw items

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
            int selectedCategory = EditorGUILayout.Popup(SelectedItemCatagoryID == -1 ? 0 : SelectedItemCatagoryID, ItemCategories.List.ToArray());
            if (selectedCategory != SelectedItemCatagoryID)
            {
                Debug.Log("On category selected with items count = "+ ItemsConfigurator.Items.Count);
                string category = ItemCategories.List[selectedCategory];
                AllItemsIDs = ItemsConfigurator.Items.Where(i=>i.Tags[0] == category).Select(x => x.ItemId).ToArray();
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

            // draw currencies
            if (Currencies != null && Currencies.Count != 0)
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
        }
    }
}
#endif
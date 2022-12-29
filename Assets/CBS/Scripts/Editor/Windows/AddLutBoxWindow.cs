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
    public class AddLootBoxWindow : AddItemWindow
    {
        private CatalogItemContainerInfo Container { get; set; }

        private ItemsConfigurator ItemsConfigurator { get; set; }

        private string SelectedItemID { get; set; }
        private int SelectedItemIndex { get; set; }
        private int SelectedCurrencyIndex { get; set; }

        private string [] AllItemsIDs { get; set; }

        private ItemsIcons Icons { get; set; }
        private CurrencyIcons CurrenciesIcons { get; set; }

        protected override void Init()
        {
            base.Init();

            Icons = CBSScriptable.Get<ItemsIcons>();
            CurrenciesIcons = CBSScriptable.Get<CurrencyIcons>();
            ItemsConfigurator = BaseConfigurator.Get<ItemsConfigurator>();
            Debug.Log("Random items at window = "+ItemsConfigurator.RandomItems.Count);
            AllItemsIDs = ItemsConfigurator.RandomItems.Select(x => x.TableId).ToArray();

            Titles[1] = "Content";
            AddTitle = "Add Loot Box";
            SaveTitle = "Save Loot Box";

            Container = CurrentData.Container ?? new CatalogItemContainerInfo();
            if (Container.ResultTableContents == null)
            {
                Container.ResultTableContents = new List<string>();
            }
            if (Container.VirtualCurrencyContents == null)
            {
                Container.VirtualCurrencyContents = new Dictionary<string, uint>();
            }
        }

        protected override void CheckInputs()
        {
            base.CheckInputs();
            CurrentData.Consumable = new CatalogItemConsumableInfo();
            CurrentData.Consumable.UsageCount = 1;
            CurrentData.Container = Container;
        }

        protected override void DrawConfigs()
        {
            GUILayout.Space(15);
            if (Container == null)
                return;

            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 14;

            // draw items

            EditorGUILayout.LabelField("Random Items", titleStyle);

            GUILayout.BeginVertical();

            GUILayout.Space(15);

            for (int i = 0; i < Container.ResultTableContents.Count; i++)
            {
                string key = Container.ResultTableContents[i];

                GUILayout.BeginHorizontal();

                GUILayout.Space(15);

                var actvieSprite = Icons.GetSprite(key);
                var iconTexture = actvieSprite == null ? null : actvieSprite.texture;
                GUILayout.Button(iconTexture, GUILayout.Width(50), GUILayout.Height(50));

                EditorGUILayout.LabelField(key);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    Container.ResultTableContents.Remove(key);
                    Container.ResultTableContents.TrimExcess();
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
                        Container.ResultTableContents.Add(SelectedItemID);
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

                if (Container.VirtualCurrencyContents != null && Container.VirtualCurrencyContents.Count != 0)
                {
                    for (int i = 0; i < Container.VirtualCurrencyContents.Count; i++)
                    {
                        string key = Container.VirtualCurrencyContents.Keys.ElementAt(i);
                        int val = (int)Container.VirtualCurrencyContents.Values.ElementAt(i);
                        GUILayout.BeginHorizontal();

                        GUILayout.Space(15);

                        var actvieSprite = CurrenciesIcons.GetSprite(key);
                        var iconTexture = actvieSprite == null ? null : actvieSprite.texture;
                        GUILayout.Button(iconTexture, GUILayout.Width(50), GUILayout.Height(50));

                        val = EditorGUILayout.IntField(key, val);

                        Container.VirtualCurrencyContents[key] = (uint)val;

                        if (GUILayout.Button("X", GUILayout.Width(20)))
                        {
                            Container.VirtualCurrencyContents.Remove(key);
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
                        if (!Container.VirtualCurrencyContents.ContainsKey(defaultKey))
                            Container.VirtualCurrencyContents[defaultKey] = 0;
                    }
                }
                GUILayout.EndHorizontal();
            }
        }
    }

}
#endif
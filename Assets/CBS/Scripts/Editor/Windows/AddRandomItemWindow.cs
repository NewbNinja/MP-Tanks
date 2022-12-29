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
    public class AddRandomItemWindow : EditorWindow
    {
        private static Action<RandomResultTable> ModifyCallback { get; set; }
        private static RandomResultTable Table { get; set; }

        private string TableID { get; set; }

        private List<ResultTableNode> Nodes;

        private bool IsInited { get; set; }
        private Vector2 ScrollView { get; set; }
        private ItemsIcons Icons { get; set; }

        private int SelectedItemCatagoryID { get; set; } = -1;
        private int SelectedCurrencyIndex { get; set; }
        private ItemsConfigurator ItemsConfigurator { get; set; }
        private Categories ItemCategories { get; set; }
        private string[] AllItemsIDs { get; set; }
        private string SelectedItemID { get; set; }
        private int SelectedItemIndex { get; set; }

        private Sprite IconSprite { get; set; }

        public static void Show(Action<RandomResultTable> modifyCallback, RandomResultTable categories)
        {
            ModifyCallback = modifyCallback;
            Table = categories;

            AddRandomItemWindow window = ScriptableObject.CreateInstance<AddRandomItemWindow>();
            window.maxSize = new Vector2(400, 700);
            window.minSize = window.maxSize;
            window.ShowUtility();
        }

        private void Hide()
        {
            this.Close();
        }

        private void OnInit()
        {
            Icons = CBSScriptable.Get<ItemsIcons>();
            ItemsConfigurator = BaseConfigurator.Get<ItemsConfigurator>();
            AllItemsIDs = ItemsConfigurator.Items.Select(x => x.ItemId).ToArray();
            ItemCategories = ItemsConfigurator.ItemsCategories;

            TableID = Table.TableId;
            Nodes = Table.Nodes ?? new List<ResultTableNode>();

            IconSprite = Icons.GetSprite(TableID);

            IsInited = true;
        }

        private void OnApply()
        {
            Table.TableId = TableID;
            Table.Nodes = Nodes;

            if (IconSprite == null)
            {
                Icons.RemoveSprite(TableID);
            }
            else
            {
                Icons.SaveSprite(TableID, IconSprite);
            }
        }

        private bool IsValidInputs()
        {
            bool validID = !string.IsNullOrEmpty(TableID);
            bool validNodes = Nodes.Count > 0;
            return validID && validNodes;
        }

        void OnGUI()
        {
            if (!IsInited)
            {
                OnInit();
            }

            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 14;

            using (var areaScope = new GUILayout.AreaScope(new Rect(0, 0, 400, 700)))
            {
                if (Table == null)
                    return;

                ScrollView = GUILayout.BeginScrollView(ScrollView);

                GUILayout.Space(10);
                GUILayout.BeginVertical();

                GUILayout.Label("Item ID", GUILayout.Width(70));
                TableID = GUILayout.TextField(TableID);
                EditorGUILayout.HelpBox("Unique ID for the item. Don't use special characters or spaces. ID must be in lower case", MessageType.Info);

                GUILayout.Space(15);
                EditorGUILayout.LabelField("Sprite", new GUILayoutOption[] { GUILayout.MaxWidth(150) });
                IconSprite = (Sprite)EditorGUILayout.ObjectField((IconSprite as UnityEngine.Object), typeof(Sprite), false, new GUILayoutOption[] { GUILayout.MaxWidth(150) });
                EditorGUILayout.HelpBox("Sprite for random item. ATTENTION! The sprite is not saved on the server, it will be included in the build", MessageType.Info);
                GUILayout.Space(15);

                EditorGUILayout.LabelField("Items", titleStyle);

                GUILayout.BeginVertical();

                GUILayout.Space(15);

                int allWeight = Nodes.Select(x => x.Weight).Sum();

                for (int i = 0; i < Nodes.Count; i++)
                {
                    string key = Nodes[i].ResultItem;

                    int weight = Nodes[i].Weight;

                    float persent = (float)weight / (float)allWeight;

                    GUILayout.BeginHorizontal();

                    GUILayout.Space(15);

                    var actvieSprite = Icons.GetSprite(key);
                    var iconTexture = actvieSprite == null ? null : actvieSprite.texture;
                    GUILayout.Button(iconTexture, GUILayout.Width(50), GUILayout.Height(50));

                    EditorGUILayout.LabelField(key, GUILayout.Width(100));

                    GUILayout.BeginVertical();

                    Nodes[i].Weight = EditorGUILayout.IntField(weight, GUILayout.Width(150));
                    if (Nodes[i].Weight < 0)
                        Nodes[i].Weight = 0;

                    float lastY = GUILayoutUtility.GetLastRect().y;

                    string progressTitle = (persent * 100).ToString("0.00") + "% chance to drop";

                    EditorGUI.ProgressBar(new Rect(75, lastY + 25, position.width - 15, 20), persent, progressTitle);

                    GUILayout.EndVertical();

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        Nodes.Remove(Nodes[i]);
                        Nodes.TrimExcess();
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
                    Debug.Log("On category selected");
                    string category = ItemCategories.List[selectedCategory];
                    AllItemsIDs = ItemsConfigurator.Items.Where(i => i.Tags[0] == category).Select(x => x.ItemId).ToArray();
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
                            Nodes.Add(new ResultTableNode {
                                ResultItem = SelectedItemID,
                                ResultItemType = ResultTableNodeType.ItemId,
                                Weight = 1
                            });
                        }
                    }
                }

                GUILayout.EndVertical();

                GUILayout.EndVertical();

                GUILayout.EndScrollView();


                GUILayout.Space(50);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Save"))
                {
                    if (IsValidInputs())
                    {
                        OnApply();
                        ModifyCallback?.Invoke(Table);
                        Hide();
                    }
                }
                if (GUILayout.Button("Close"))
                {
                    Hide();
                }
                GUILayout.EndHorizontal();

                
            }
        }
    }
}
#endif
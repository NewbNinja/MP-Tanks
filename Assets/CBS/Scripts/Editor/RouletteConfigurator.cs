#if ENABLE_PLAYFABADMIN_API
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.ServerModels;
using UnityEditor;
using System.Linq;
using System;
using CBS.Scriptable;
using CBS.Editor.Window;

namespace CBS.Editor
{
    public class RouletteConfigurator : BaseConfigurator
    {
        private readonly string ROULETTE_TITLE_ID = CBSConstants.RouletteTitleKey;

        private RouletteTable RouletteTable { get; set; } = new RouletteTable();

        private List<PlayFab.AdminModels.CatalogItem> CachedItems { get; set; }
        private Categories CachedItemCategories { get; set; }
        private Categories CachedLootBoxCategories { get; set; }
        private List<string> CacheCurrencies { get; set; }

        private ItemsIcons Icons { get; set; }

        private GUILayoutOption[] AddButtonOptions
        {
            get
            {
                return new GUILayoutOption[] { GUILayout.Height(30), GUILayout.Width(120) };
            }
        }

        private GUILayoutOption[] RemoveButtonOptions
        {
            get
            {
                return new GUILayoutOption[] { GUILayout.Height(30), GUILayout.Width(170) };
            }
        }

        private GUILayoutOption[] SaveButtonOptions
        {
            get
            {
                return new GUILayoutOption[] { GUILayout.Width(50) };
            }
        }

        protected override string Title => "Roulette";

        protected override bool DrawScrollView => true;

        private EditorData EditorData { get; set; }

        public override void Init(MenuTitles title)
        {
            base.Init(title);
            Icons = CBSScriptable.Get<ItemsIcons>();
            EditorData = CBSScriptable.Get<EditorData>();
            GetRouletteTable();
        }

        protected override void OnDrawInside()
        {
            // draw level table
            if (RouletteTable != null && RouletteTable.Positions != null)
            {
                var levelTitleStyle = new GUIStyle(GUI.skin.label);
                levelTitleStyle.fontStyle = FontStyle.Bold;
                levelTitleStyle.fontSize = 14;

                EditorGUILayout.LabelField("Roulette table", levelTitleStyle);
                GUILayout.Space(20);

                int allWeight = RouletteTable.Positions.Select(x => x.Weight).Sum();

                for (int i=0;i< RouletteTable.Positions.Count; i++)
                {
                    var position = RouletteTable.Positions[i];
                    GUILayout.BeginHorizontal();
                    string dayString = (i+1).ToString();
                    var positionDetail = position;

                    EditorGUILayout.LabelField(dayString, levelTitleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(20) });

                    var actvieSprite = Icons.GetSprite(position.ID);
                    var iconTexture = actvieSprite == null ? null : actvieSprite.texture;
                    GUILayout.Button(iconTexture, GUILayout.Width(50), GUILayout.Height(50));

                    EditorGUILayout.LabelField(position.DisplayName, new GUILayoutOption[] { GUILayout.MaxWidth(200) });

                    // draw progress
                    int weight = position.Weight;
                    float persent = (float)weight / (float)allWeight;

                    
                    float lastY = GUILayoutUtility.GetLastRect().y;
                    float lastX = GUILayoutUtility.GetLastRect().x;

                    string progressTitle = (persent * 100).ToString("0.00") + "% to drop";

                    EditorGUI.ProgressBar(new Rect(lastX, lastY + 20, 200, 30), persent, progressTitle);

                    GUILayout.FlexibleSpace();

                    GUILayout.Space(50);
                    if (EditorUtils.DrawButton("Configurate prize", EditorData.AddPrizeColor, 12, new GUILayoutOption[] { GUILayout.MaxWidth(150) }))
                    {
                        ShowRouletteDialog(position, ItemAction.EDIT, onAdd => {
                            UpdateRouletteData(RouletteTable);
                        });
                    }

                    // draw save button
                    if (EditorUtils.DrawButton("Remove", EditorData.RemoveColor, 12, new GUILayoutOption[] { GUILayout.MaxWidth(100) }))
                    {
                        RemoveBonusDetail(position);
                    }

                    GUILayout.EndHorizontal();

                    EditorUtils.DrawUILine(Color.grey, 1, 20);

                    GUILayout.Space(10);
                }

                GUILayout.Space(20);

                GUILayout.BeginHorizontal();
                // add new level
                if (EditorUtils.DrawButton("Add position", EditorData.AddColor, 12, AddButtonOptions))
                {
                    ShowRouletteDialog(new RoulettePosition(), ItemAction.ADD, onAdd => {
                        AddNewRoulettePosition(onAdd);
                    });
                }

                if (RouletteTable.Positions.Count != 0)
                {
                    // remove last level
                    if (EditorUtils.DrawButton("Remove last position", EditorData.RemoveColor, 12, RemoveButtonOptions))
                    {
                        if (RouletteTable == null || RouletteTable.Positions.Count == 0)
                            return;
                        int lastLevelKey = RouletteTable.Positions.Count - 1;
                        RemoveBonusDetail(lastLevelKey);
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }
        
        // get level table
        private void GetRouletteTable()
        {
            ShowProgress();
            var keyList = new List<string>();
            keyList.Add(ROULETTE_TITLE_ID);
            var dataRequest = new PlayFab.AdminModels.GetTitleDataRequest {
                Keys = keyList
            };

            PlayFabAdminAPI.GetTitleData(dataRequest, OnLevelTableGetted, OnLevelTableError);
        }

        private void OnLevelTableGetted(PlayFab.AdminModels.GetTitleDataResult result)
        {
            bool tableExist = result.Data.ContainsKey(ROULETTE_TITLE_ID);
            if (tableExist)
            {
                string tableRaw = result.Data[ROULETTE_TITLE_ID];
                var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                var table = jsonPlugin.DeserializeObject<RouletteTable>(tableRaw);
                table.Positions = table.Positions ?? new List<RoulettePosition>();
                HideProgress();
                RouletteTable = table;
            }
            else
            {
                RouletteTable = new RouletteTable();
                RouletteTable.Positions = new List<RoulettePosition>();
            }
            HideProgress();
        }

        private void OnLevelTableError(PlayFabError error)
        {
            HideProgress();
            AddErrorLog(error);
        }

        // add empty level
        private void AddNewRoulettePosition(RoulettePosition position)
        {
            RouletteTable.Positions.Add(position);
            UpdateRouletteData(RouletteTable);
        }

        private void UpdateRouletteData(RouletteTable rouletteTable)
        {
            ShowProgress();
            var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
            string listRaw = jsonPlugin.SerializeObject(rouletteTable);

            var dataRequest = new SetTitleDataRequest
            {
                Key = ROULETTE_TITLE_ID,
                Value = listRaw
            };

            PlayFabServerAPI.SetTitleData(dataRequest, OnLevelDataUpdated, OnUpdateLevelDataFailed);
        }

        private void RemoveBonusDetail(int index)
        {
            ShowProgress();

            RouletteTable.Positions.RemoveAt(index);
            RouletteTable.Positions.TrimExcess();

            UpdateRouletteData(RouletteTable);
        }

        private void RemoveBonusDetail(RoulettePosition position)
        {
            ShowProgress();

            RouletteTable.Positions.Remove(position);
            RouletteTable.Positions.TrimExcess();

            UpdateRouletteData(RouletteTable);
        }

        private void OnLevelDataUpdated(SetTitleDataResult result)
        {
            HideProgress();
            GetRouletteTable();
        }

        private void OnUpdateLevelDataFailed(PlayFabError error)
        {
            HideProgress();
            AddErrorLog(error);
        }

        private void ShowRouletteDialog(RoulettePosition prize, ItemAction action, Action<RoulettePosition> modifyCallback)
        {
            if (CachedItemCategories == null || CachedItems == null || CacheCurrencies == null || CachedLootBoxCategories == null)
            {
                ShowProgress();
                var itemConfig = new ItemsConfigurator();
                itemConfig.GetCategories(categoriesResult => {
                    if (categoriesResult.Data.ContainsKey(CBSConstants.CategoriesKey))
                    {
                        var rawData = categoriesResult.Data[CBSConstants.CategoriesKey];
                        CachedItemCategories = JsonUtility.FromJson<Categories>(rawData);
                    }
                    else
                    {
                        CachedItemCategories = new Categories();
                    }

                    if (categoriesResult.Data.ContainsKey(CBSConstants.LootboxesCategoriesKey))
                    {
                        var rawData = categoriesResult.Data[CBSConstants.LootboxesCategoriesKey];
                        CachedLootBoxCategories = JsonUtility.FromJson<Categories>(rawData);
                    }
                    else
                    {
                        CachedLootBoxCategories = new Categories();
                    }

                    // get item catalog
                    itemConfig.GetItemsCatalog(itemsResult => {
                        HideProgress();
                        CachedItems = itemsResult.Catalog;
                        itemConfig.GetAllCurrencies(curResult => {
                            CacheCurrencies = curResult.VirtualCurrencies.Select(x => x.CurrencyCode).ToList();
                            // show prize windows
                            AddRouletteWindow.Show(new RouletteWindowRequest
                            {
                                currencies = CacheCurrencies,
                                includeCurencies = true,
                                itemCategories = CachedItemCategories,
                                lootboxCategories = CachedLootBoxCategories,
                                items = CachedItems,
                                modifyCallback = modifyCallback,
                                position = prize,
                                Action = action
                            });
                            //GUIUtility.ExitGUI();
                        });
                    });
                });
            }
            else
            {
                // show prize windows
                AddRouletteWindow.Show(new RouletteWindowRequest
                {
                    currencies = CacheCurrencies,
                    includeCurencies = true,
                    itemCategories = CachedItemCategories,
                    lootboxCategories = CachedLootBoxCategories,
                    items = CachedItems,
                    modifyCallback = modifyCallback,
                    position = prize,
                    Action = action
                });
                GUIUtility.ExitGUI();
            }
        }
    }
}
#endif
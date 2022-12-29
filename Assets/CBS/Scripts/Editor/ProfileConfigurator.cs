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
    public class ProfileConfigurator : BaseConfigurator
    {
        private readonly string LEVEL_TITLE_ID = CBSConstants.LevelTitleKey;

        private LevelTable LevelTable { get; set; } = new LevelTable();

        private List<PlayFab.AdminModels.CatalogItem> CachedItems { get; set; }
        private Categories CachedItemCategories { get; set; }
        private Categories CachedLootBoxCategories { get; set; }
        private List<string> CacheCurrencies { get; set; }

        private EditorData EditorData { get; set; }

        private GUILayoutOption[] AddButtonOptions
        {
            get
            {
                return new GUILayoutOption[] { GUILayout.Height(36.3f), GUILayout.Width(164f) };
            }
        }

        private GUILayoutOption[] SaveButtonOptions
        {
            get
            {
                return new GUILayoutOption[] { GUILayout.Width(50) };
            }
        }

        protected override string Title => "Profile Congiguration";

        protected override bool DrawScrollView => true;

        public override void Init(MenuTitles title)
        {
            base.Init(title);
            EditorData = CBSScriptable.Get<EditorData>();
            GetLevelTable();
        }

        protected override void OnDrawInside()
        {
            GUIStyle btnStyle = new GUIStyle("Label");

            // draw level table
            if (LevelTable != null)
            {
                var titleStyle = new GUIStyle(GUI.skin.label);
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.fontSize = 16;

                EditorGUILayout.LabelField("Expirience table", titleStyle);
                GUILayout.Space(20);

                var levelTitleStyle = new GUIStyle(GUI.skin.label);
                levelTitleStyle.fontStyle = FontStyle.Bold;

                if (LevelTable.Table.Count == 0)
                {
                    // import default preset
                    if (EditorUtils.DrawButton("Import default level data", EditorData.AddColor, 12, new GUILayoutOption[] { GUILayout.Height(30), GUILayout.Width(200) }))
                    {
                        ImportDefaultLevels();
                    }
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("0", levelTitleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(20), GUILayout.Height(35) });
                    EditorGUILayout.LabelField("Null level is not configurable", new GUILayoutOption[] { GUILayout.MaxWidth(270), GUILayout.Height(35) });
                    GUILayout.Space(160);
                    if (EditorUtils.DrawButton("Grant items on registration", EditorData.AddColor, 16, new GUILayoutOption[] { GUILayout.Width(250), GUILayout.Height(35) }))
                    {
                        var prize = LevelTable.RegistrationPrize;
                        ShowPrizeDialog(prize, false, result => {
                            LevelTable.RegistrationPrize = result;
                            UpdateLevelDetail(LevelTable);
                        });
                    }
                    GUILayout.EndHorizontal();
                    EditorUtils.DrawUILine(Color.grey, 1, 20);
                }

                for (int i=0;i< LevelTable.Table.Count; i++)
                {
                    var level = LevelTable.Table[i];
                    GUILayout.BeginHorizontal();
                    string levelString = (i+1).ToString();
                    var levelDetail = level;
                    // display exp
                    
                    EditorGUILayout.LabelField(levelString, levelTitleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(20) });
                    EditorGUILayout.LabelField("Experience is needed to reach the level "+ levelString, new GUILayoutOption[] { GUILayout.MaxWidth(270) });
                    GUIStyle expStyle = new GUIStyle("Label");
                    var texture = ResourcesUtils.GetTextureByPath("Profile/exp.png");
                    GUILayout.BeginVertical(new GUILayoutOption[] { GUILayout.MaxWidth(40) });
                    GUILayout.Space(-10);
                    GUILayout.Button(texture, expStyle, new GUILayoutOption[] { GUILayout.MaxWidth(40) });
                    GUILayout.EndVertical();
                    levelDetail.Expirience = EditorGUILayout.IntField(levelDetail.Expirience, new GUILayoutOption[] { GUILayout.MaxWidth(150) });
                    // draw save button
                    if (EditorUtils.DrawButton("Save", EditorData.SaveColor, 12, SaveButtonOptions))
                    {
                        //levelDetail.Expirience = exp;
                        var rawDataToSave = JsonUtility.ToJson(levelDetail);
                        UpdateLevelDetail(LevelTable);
                    }
                    // draw price button
                    GUILayout.Space(50);

                    if (EditorUtils.DrawButton("Edit prize", EditorData.AddPrizeColor, 12, new GUILayoutOption[] { GUILayout.MaxWidth(100) }))
                    {
                        ShowPrizeDialog(level.Prizes, true, result => {
                            level.Prizes = result;
                            UpdateLevelDetail(LevelTable);
                        });
                    }

                    GUILayout.EndHorizontal();
                    EditorUtils.DrawUILine(Color.grey, 1, 20);
                    GUILayout.Space(10);
                }

                GUILayout.Space(20);

                GUILayout.BeginHorizontal();
                // add new level
                if (EditorUtils.DrawButton("Add new level", EditorData.AddColor, 14, AddButtonOptions))
                {
                    AddNewLevel();
                }

                if (LevelTable.Table.Count != 0)
                {
                    // remove last level
                    if (EditorUtils.DrawButton("Remove last level", EditorData.RemoveColor, 14, AddButtonOptions))
                    {
                        if (LevelTable == null || LevelTable.Table.Count == 0)
                            return;
                        int lastLevelKey = LevelTable.Table.Count - 1;
                        RemoveLevelDetail(lastLevelKey);
                    }
                    // remove all level
                    if (EditorUtils.DrawButton("Remove all levels", EditorData.RemoveColor, 14, AddButtonOptions))
                    {
                        int option = EditorUtility.DisplayDialogComplex("Warning",
                            "Are you sure you want to remove all levels data?",
                            "Yes",
                            "No",
                            string.Empty);
                        switch (option)
                        {
                            // ok.
                            case 0:
                                RemoveLevelGroup();
                                break;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }
        
        // get level table
        private void GetLevelTable()
        {
            ShowProgress();
            var keyList = new List<string>();
            keyList.Add(LEVEL_TITLE_ID);
            var dataRequest = new PlayFab.AdminModels.GetTitleDataRequest {
                Keys = keyList
            };

            PlayFabAdminAPI.GetTitleData(dataRequest, OnLevelTableGetted, OnLevelTableError);
        }

        private void OnLevelTableGetted(PlayFab.AdminModels.GetTitleDataResult result)
        {
            bool tableExist = result.Data.ContainsKey(LEVEL_TITLE_ID);
            if (tableExist)
            {
                string tableRaw = result.Data[LEVEL_TITLE_ID];
                var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                var table = jsonPlugin.DeserializeObject<LevelTable>(tableRaw);
                HideProgress();
                LevelTable = table;
            }
            else
            {
                LevelTable = new LevelTable();
            }
            HideProgress();
        }

        private void OnLevelTableError(PlayFabError error)
        {
            HideProgress();
            AddErrorLog(error);
        }

        // remove level group
        private void RemoveLevelGroup()
        {
            ShowProgress();
            LevelTable = new LevelTable();
            UpdateLevelDetail(LevelTable);
        }

        // add empty level
        private void AddNewLevel()
        {
            var levelDetail = new LevelDetail
            {
                Expirience = 0
            };
            LevelTable.Table.Add(levelDetail);
            UpdateLevelDetail(LevelTable);
        }

        private void UpdateLevelDetail(LevelTable levelTable)
        {
            ShowProgress();
            var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
            string listRaw = jsonPlugin.SerializeObject(levelTable);

            var dataRequest = new SetTitleDataRequest
            {
                Key = LEVEL_TITLE_ID,
                Value = listRaw
            };

            PlayFabServerAPI.SetTitleData(dataRequest, OnLevelDataUpdated, OnUpdateLevelDataFailed);
        }

        private void RemoveLevelDetail(int index)
        {
            ShowProgress();

            LevelTable.Table.RemoveAt(index);
            LevelTable.Table.TrimExcess();

            UpdateLevelDetail(LevelTable);
        }

        private void OnLevelDataUpdated(SetTitleDataResult result)
        {
            HideProgress();
            GetLevelTable();
        }

        private void OnUpdateLevelDataFailed(PlayFabError error)
        {
            HideProgress();
            AddErrorLog(error);
        }

        private void ShowPrizeDialog(PrizeObject prize, bool includeCurrencies, Action<PrizeObject> modifyCallback)
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
                            AddPrizeWindow.Show(new PrizeWindowRequest
                            {
                                currencies = CacheCurrencies,
                                includeCurencies = includeCurrencies,
                                itemCategories = CachedItemCategories,
                                lootboxCategories = CachedLootBoxCategories,
                                items = CachedItems,
                                modifyCallback = modifyCallback,
                                prize = prize
                            });
                            //GUIUtility.ExitGUI();
                        });
                    });
                });
            }
            else
            {
                // show prize windows
                AddPrizeWindow.Show(new PrizeWindowRequest
                {
                    currencies = CacheCurrencies,
                    includeCurencies = includeCurrencies,
                    itemCategories = CachedItemCategories,
                    lootboxCategories = CachedLootBoxCategories,
                    items = CachedItems,
                    modifyCallback = modifyCallback,
                    prize = prize
                });
                GUIUtility.ExitGUI();
            }
        }

        // default set
        private void ImportDefaultLevels()
        {
            var defaultPreset = GetDefaultPreset();
            UpdateLevelDetail(defaultPreset);
        }

        private LevelTable GetDefaultPreset()
        {
            var newSet = new LevelTable();

            newSet.Table.Add(new LevelDetail { Expirience = 100 });
            newSet.Table.Add(new LevelDetail { Expirience = 200 });
            newSet.Table.Add(new LevelDetail { Expirience = 500 });
            newSet.Table.Add(new LevelDetail { Expirience = 1000 });
            newSet.Table.Add(new LevelDetail { Expirience = 1700 });
            newSet.Table.Add(new LevelDetail { Expirience = 3000 });
            newSet.Table.Add(new LevelDetail { Expirience = 5000 });
            newSet.Table.Add(new LevelDetail { Expirience = 8000 });
            newSet.Table.Add(new LevelDetail { Expirience = 15000 });
            newSet.Table.Add(new LevelDetail { Expirience = 30000 });

            return newSet;
        }
    }
}
#endif

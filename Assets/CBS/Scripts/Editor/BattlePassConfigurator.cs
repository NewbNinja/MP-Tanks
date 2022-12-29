#if ENABLE_PLAYFABADMIN_API
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.AdminModels;
using UnityEditor;
using System.Linq;
using System;
using CBS.Playfab;
using CBS.Editor.Window;
using CBS.Utils;
using CBS.Scriptable;

namespace CBS.Editor
{
    public class BattlePassConfigurator : BaseConfigurator
    {
        protected override string Title => "BattlePass Congiguration";

        protected override bool DrawScrollView => true;

        private Rect CategoriesRect = new Rect(0, 0, 150, 700);
        private Rect ItemsRect = new Rect(200, 100, 835, 700);
        private Vector2 PositionScroll { get; set; }
        private readonly int DefaultPositionCount = 10;
        private LevelTreeType CurrentRewardView;

        private BattlePassData BattlePassData { get; set; } = new BattlePassData();
        private BattlePassInstance SelectedInstance { get; set; }
        private int BattlePassIndex { get; set; }
        private int SelectedToolBar { get; set; }

        private List<CatalogItem> CachedItems { get; set; }
        private Categories CachedItemCategories { get; set; }
        private List<string> CacheCurrencies { get; set; }
        private Categories CachedLootBoxCategories { get; set; }

        private EditorData EditorData { get; set; }
        private ObjectCustomDataDrawer<CBSBattlePassCustomData> CustomDataDrawer { get; set; }

        private GUILayoutOption[] AddButtonOptions
        {
            get
            {
                return new GUILayoutOption[] { GUILayout.Height(30), GUILayout.Width(120) };
            }
        }

        public override void Init(MenuTitles title)
        {
            base.Init(title);
            EditorData = CBSScriptable.Get<EditorData>();
            CustomDataDrawer = new ObjectCustomDataDrawer<CBSBattlePassCustomData>();
            GetBattlePassData();
        }

        protected override void OnDrawInside()
        {
            DrawTitles();
            DrawBattlePassInstanes();
        }

        private void DrawToolbar()
        {
            SelectedToolBar = GUILayout.Toolbar(SelectedToolBar, new string[] { "Default rewards", "Premium rewards" });
            switch (SelectedToolBar)
            {
                case 0:
                    CurrentRewardView = LevelTreeType.Default;
                    break;
                case 1:
                    CurrentRewardView = LevelTreeType.Premium;
                    break;
                default:
                    break;
            }
        }

        private void DrawTitles()
        {
            using (var areaScope = new GUILayout.AreaScope(CategoriesRect))
            {
                GUILayout.BeginVertical();

                int categoryHeight = 30;
                int categoriesCount = BattlePassData.Instances == null ? 0 : BattlePassData.Instances.Count;

                if (BattlePassData.Instances != null && BattlePassData.Instances.Count > 0)
                {
                    var categoriesMenu = BattlePassData.Instances.Select(x => x.DisplayName).ToArray();
                    BattlePassIndex = GUI.SelectionGrid(new Rect(0, 100, 150, categoryHeight * categoriesCount), BattlePassIndex, categoriesMenu.ToArray(), 1);
                    string selctedCategory = categoriesMenu[BattlePassIndex];

                    SelectedInstance = BattlePassData.Instances.ElementAt(BattlePassIndex);
                }

                GUILayout.Space(30);
                GUILayout.Space(30);
                var oldColor = GUI.color;
                GUI.backgroundColor = EditorData.AddColor;
                var style = new GUIStyle(GUI.skin.button);
                style.fontStyle = FontStyle.Bold;
                style.fontSize = 12;
                if (GUI.Button(new Rect(0, 130 + categoryHeight * categoriesCount, 150, categoryHeight), "Add new Instance", style))
                {
                    AddBattlePassInstanceWindow.Show(onAdd =>
                    {
                        var newInstance = onAdd;
                        BattlePassData.Instances.Add(newInstance);
                        SaveBattlePass(BattlePassData);
                    });
                    GUIUtility.ExitGUI();
                }
                GUI.backgroundColor = oldColor;

                GUILayout.EndVertical();
            }
        }

        private void DrawBattlePassInstanes()
        {
            if (SelectedInstance == null)
                return;
            using (var areaScope = new GUILayout.AreaScope(ItemsRect))
            {
                PositionScroll = GUILayout.BeginScrollView(PositionScroll);

                var titleStyle = new GUIStyle(GUI.skin.label);
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.fontSize = 14;

                EditorGUILayout.LabelField("BattlePass ID", titleStyle);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(SelectedInstance.ID);

                GUILayout.FlexibleSpace();

                if (EditorUtils.DrawButton("Remove", EditorData.RemoveColor, 12, new GUILayoutOption[] { GUILayout.Width(100), GUILayout.Height(30) }))
                {
                    int option = EditorUtility.DisplayDialogComplex("Warning",
                            "Are you sure you want to remove this instance?",
                            "Yes",
                            "No",
                            string.Empty);
                    switch (option)
                    {
                        // ok.
                        case 0:
                            RemoveBattlePassInstance(SelectedInstance);
                            SelectedInstance = null;
                            SaveBattlePass(BattlePassData);
                            break;
                    }
                    if (SelectedInstance == null)
                    {
                        BattlePassIndex = 0;
                        return;
                    }
                }

                if (EditorUtils.DrawButton("Save", EditorData.SaveColor, 12, new GUILayoutOption[] { GUILayout.Width(100), GUILayout.Height(30) }))
                {
                    SaveBattlePass(BattlePassData);
                }

                GUILayout.EndHorizontal();

                // draw state
                SelectedInstance.State = EditorUtils.DrawDevelopmentState(SelectedInstance.State);
                EditorGUILayout.HelpBox("In [IN_DEVELOP] state, instance will not be visible to players.", MessageType.Info);
                GUILayout.Space(10);

                // draw name
                EditorGUILayout.LabelField("Display Name", titleStyle);
                SelectedInstance.DisplayName = EditorGUILayout.TextField(SelectedInstance.DisplayName, new GUILayoutOption[] { GUILayout.Width(400) });
                GUILayout.Space(10);

                // draw description
                var descriptionTitle = new GUIStyle(GUI.skin.textField);
                descriptionTitle.wordWrap = true;
                EditorGUILayout.LabelField("Description", titleStyle);
                SelectedInstance.Description = EditorGUILayout.TextArea(SelectedInstance.Description, descriptionTitle, new GUILayoutOption[] { GUILayout.Height(150) });
                GUILayout.Space(10);

                // draw exp step
                EditorGUILayout.LabelField("Experience step", titleStyle);
                var expStep = EditorGUILayout.IntField(SelectedInstance.ExpStep, new GUILayoutOption[] { GUILayout.Width(150) });
                if (expStep < 1)
                    expStep = 1;
                SelectedInstance.ExpStep = expStep;
                EditorGUILayout.HelpBox("Describes how much experience the player needs to gain in order to reach the next level.", MessageType.Info);
                GUILayout.Space(10);

                EditorUtils.DrawUILine(Color.grey, 2, 20);
                GUILayout.Space(10);

                // draw date
                var perion = SelectedInstance.Duration;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Start Date", titleStyle);
                GUILayout.Space(100);
                EditorGUILayout.LabelField("End Date", titleStyle);
                GUILayout.EndVertical();
                GUILayout.Space(2);
                GUILayout.BeginHorizontal();
                perion.Start = EditorUtils.DrawDateTimeField(perion.Start);
                GUILayout.Space(100);
                perion.End = EditorUtils.DrawDateTimeField(perion.End);
                GUILayout.EndVertical();
                EditorGUILayout.HelpBox("Please note, server time is used.", MessageType.Info);
                GUILayout.Space(10);

                EditorUtils.DrawUILine(Color.grey, 2, 20);
                GUILayout.Space(10);

                // draw customs properties
                EditorGUILayout.LabelField("Custom Data", titleStyle);
                var rawData = CustomDataDrawer.Draw(SelectedInstance);
                GUILayout.Space(10);

                EditorUtils.DrawUILine(Color.grey, 2, 20);

                // draw levels 
                EditorGUILayout.LabelField("Level tree", titleStyle);
                var levelTree = SelectedInstance.LevelTree ?? new List<BattlePassLevel>();
                DrawToolbar();
                for (int i=0;i< levelTree.Count;i++)
                {
                    
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    var level = levelTree[i];
                    var rewardObject = CurrentRewardView == LevelTreeType.Default ? level.DefaultReward : level.PremiumReward;
                    rewardObject = rewardObject ?? new PrizeObject();

                    EditorUtils.DrawButton("Level " + (i + 1), EditorData.EditColor, 12, new GUILayoutOption[] { GUILayout.Width(120), GUILayout.Height(50) });
                    GUILayout.Space(10);

                    // draw rewards
                    EditorUtils.DrawReward(rewardObject, 50, ItemDirection.NONE);
                    GUILayout.FlexibleSpace();

                    // draw add button
                    if (EditorUtils.DrawButton("+ Reward", EditorData.AddPrizeColor, 12, new GUILayoutOption[] { GUILayout.Height(50), GUILayout.Width(90) }))
                    {
                        ShowPrizeDialog(rewardObject, true, result => {
                            if (CurrentRewardView == LevelTreeType.Default)
                            {
                                level.DefaultReward = result;
                            }
                            else
                            {
                                level.PremiumReward = result;
                            }
                        });
                    }

                    GUILayout.EndHorizontal();
                }

                // draw level buttons
                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                if (EditorUtils.DrawButton("Add +1 level", EditorData.AddColor, 12, AddButtonOptions))
                {
                    AddLevels(1);
                }

                if (EditorUtils.DrawButton("Add +10 levels", EditorData.AddColor, 12, AddButtonOptions))
                {
                    AddLevels(10);
                }
                GUILayout.EndVertical();

                GUILayout.Space(110);

                GUILayout.EndScrollView();
            }
        }

        private void AddLevels(int count)
        {
            for (int i=0;i<count;i++)
            {
                var tree = SelectedInstance.LevelTree ?? new List<BattlePassLevel>();
                tree.Add(new BattlePassLevel());
                SelectedInstance.LevelTree = tree;
            }
        }

        private void GetBattlePassData()
        {
            ShowProgress();
            var keys = new List<string>();
            keys.Add(CBSConstants.BattlePassDataKey);

            var request = new GetTitleDataRequest
            {
                Keys = keys
            };
            PlayFabAdminAPI.GetTitleInternalData(request, OnInternalDataGetted, OnGetDataFailed);
        }

        private void OnInternalDataGetted(GetTitleDataResult result)
        {
            HideProgress();
            var dictionary = result.Data;
            bool keyExist = dictionary.ContainsKey(CBSConstants.BattlePassDataKey);
            var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
            Debug.Log("Loaded raw data "+ dictionary[CBSConstants.BattlePassDataKey]);
            BattlePassData = keyExist ? jsonPlugin.DeserializeObject<BattlePassData>(dictionary[CBSConstants.BattlePassDataKey]) : new BattlePassData();
        }

        private void OnGetDataFailed(PlayFabError error)
        {
            AddErrorLog(error);
            HideProgress();
        }

        private void SaveBattlePass(BattlePassData battlePassData)
        {
            ShowProgress();

            var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
            
            string rawData = jsonPlugin.SerializeObject(battlePassData);
            Debug.Log("Saved json " + rawData);

            var request = new SetTitleDataRequest
            {
                Key = CBSConstants.BattlePassDataKey,
                Value = rawData
            };
            PlayFabAdminAPI.SetTitleInternalData(request, OnSaveBattlePass, OnSaveDataFailed);
        }

        private void OnSaveBattlePass(SetTitleDataResult result)
        {
            HideProgress();
            GetBattlePassData();
        }

        private void OnSaveDataFailed(PlayFabError error)
        {
            AddErrorLog(error);
            HideProgress();
        }

        private void RemoveBattlePassInstance(BattlePassInstance instance)
        {
            if (BattlePassData.Instances.Contains(instance))
            {
                BattlePassData.Instances.Remove(instance);
            }
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

        public enum LevelTreeType
        {
            Default,
            Premium
        }
    }
}
#endif

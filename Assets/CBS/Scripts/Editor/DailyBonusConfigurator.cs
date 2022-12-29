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
    public class DailyBonusConfigurator : BaseConfigurator
    {
        private readonly string BONUS_TITLE_ID = CBSConstants.DailyBonusTitleKey;

        private DailyBonusTable BonusTable { get; set; } = new DailyBonusTable();

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

        private GUILayoutOption[] SaveButtonOptions
        {
            get
            {
                return new GUILayoutOption[] { GUILayout.Width(50) };
            }
        }

        protected override string Title => "Daily Bonus";

        protected override bool DrawScrollView => true;

        private EditorData EditorData { get; set; }

        public override void Init(MenuTitles title)
        {
            base.Init(title);
            EditorData = CBSScriptable.Get<EditorData>();
            Icons = CBSScriptable.Get<ItemsIcons>();
            GetBonusTable();
        }

        protected override void OnDrawInside()
        {
            // draw level table
            if (BonusTable != null && BonusTable.DaliyPrizes != null)
            {
                var levelTitleStyle = new GUIStyle(GUI.skin.label);
                levelTitleStyle.fontStyle = FontStyle.Bold;
                levelTitleStyle.fontSize = 14;

                EditorGUILayout.LabelField("Bonus table", levelTitleStyle);
                GUILayout.Space(20);

                for (int i=0;i< BonusTable.DaliyPrizes.Count; i++)
                {
                    var prize = BonusTable.DaliyPrizes[i];
                    GUILayout.BeginHorizontal();
                    string dayString = (i+1).ToString() + " day";
                    var levelDetail = prize;

                    EditorGUILayout.LabelField(dayString, levelTitleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(100) });

                    GUILayout.FlexibleSpace();

                    GUILayout.BeginVertical();
                    // draw price button
                    //GUILayout.Space(50);
                    if (EditorUtils.DrawButton("Configure prize", EditorData.AddPrizeColor, 12, new GUILayoutOption[] { GUILayout.Width(130) }))
                    {
                        ShowPrizeDialog(prize, true, result => {
                            UpdateBonusDetail(BonusTable);
                        });
                    }

                    // draw save button
                    if (EditorUtils.DrawButton("Save", EditorData.SaveColor, 12, new GUILayoutOption[] { GUILayout.Width(130) }))
                    {
                        var rawDataToSave = JsonUtility.ToJson(levelDetail);
                        UpdateBonusDetail(BonusTable);
                    }
                    GUILayout.EndVertical();

                    GUILayout.EndHorizontal();

                    // draw items
                    /*GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                    var items = prize.BundledItems ?? new List<string>();
                    var lootboxes = prize.Lootboxes ?? new List<string>();
                    var allItems = items.Concat(lootboxes).ToArray();

                    for (int j = 0;j< allItems.Length; j++)
                    {
                        var itemID = allItems[j];
                        if (!string.IsNullOrEmpty(itemID))
                        {
                            // draw icon
                            var actvieSprite = Icons.GetSprite(itemID);
                            var iconTexture = actvieSprite == null ? null : actvieSprite.texture;
                            GUILayout.Button(iconTexture, GUILayout.Width(50), GUILayout.Height(50));
                        }
                    }

                    // draw currencies
                    var curList = prize.BundledVirtualCurrencies;
                    if (curList != null)
                    {
                        foreach (var currency in curList)
                        {
                            var curSprite = CBSScriptable.Get<CurrencyIcons>().GetSprite(currency.Key);
                            var curTexture = curSprite == null ? null : curSprite.texture;
                            GUILayout.Button(curTexture, GUILayout.Width(50), GUILayout.Height(50));
                            var textDimensions = GUI.skin.label.CalcSize(new GUIContent(currency.Value.ToString()));
                            //EditorGUILayout.LabelField(currency.Key, GUILayout.MaxWidth(20));
                            EditorGUILayout.LabelField(currency.Value.ToString(), GUILayout.Width(textDimensions.x));
                        }
                    }

                    GUILayout.EndHorizontal();*/

                    EditorUtils.DrawReward(prize, 50, ItemDirection.HORIZONTAL);

                    EditorUtils.DrawUILine(Color.grey, 1, 20);

                    GUILayout.Space(10);
                }

                GUILayout.Space(20);

                GUILayout.BeginHorizontal();
                // add new level
                if (EditorUtils.DrawButton("Add new day", EditorData.AddColor, 12, AddButtonOptions))
                {
                    AddNewDay();
                }

                if (BonusTable.DaliyPrizes.Count != 0)
                {
                    // remove last level
                    if (EditorUtils.DrawButton("Remove last day", EditorData.RemoveColor, 12, AddButtonOptions))
                    {
                        if (BonusTable == null || BonusTable.DaliyPrizes.Count == 0)
                            return;
                        int lastLevelKey = BonusTable.DaliyPrizes.Count - 1;
                        RemoveBonusDetail(lastLevelKey);
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }
        
        // get level table
        private void GetBonusTable()
        {
            ShowProgress();
            var keyList = new List<string>();
            keyList.Add(BONUS_TITLE_ID);
            var dataRequest = new PlayFab.AdminModels.GetTitleDataRequest {
                Keys = keyList
            };

            PlayFabAdminAPI.GetTitleData(dataRequest, OnLevelTableGetted, OnLevelTableError);
        }

        private void OnLevelTableGetted(PlayFab.AdminModels.GetTitleDataResult result)
        {
            bool tableExist = result.Data.ContainsKey(BONUS_TITLE_ID);
            if (tableExist)
            {
                string tableRaw = result.Data[BONUS_TITLE_ID];
                var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                var table = jsonPlugin.DeserializeObject<DailyBonusTable>(tableRaw);
                table.DaliyPrizes = table.DaliyPrizes ?? new List<PrizeObject>();
                HideProgress();
                BonusTable = table;
            }
            else
            {
                BonusTable = new DailyBonusTable();
                BonusTable.DaliyPrizes = new List<PrizeObject>();
            }
            HideProgress();
        }

        private void OnLevelTableError(PlayFabError error)
        {
            HideProgress();
            AddErrorLog(error);
        }

        // add empty level
        private void AddNewDay()
        {
            var dayDetail = new PrizeObject();
            BonusTable.DaliyPrizes.Add(dayDetail);
            UpdateBonusDetail(BonusTable);
        }

        private void UpdateBonusDetail(DailyBonusTable levelTable)
        {
            ShowProgress();
            var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
            string listRaw = jsonPlugin.SerializeObject(levelTable);

            var dataRequest = new SetTitleDataRequest
            {
                Key = BONUS_TITLE_ID,
                Value = listRaw
            };

            PlayFabServerAPI.SetTitleData(dataRequest, OnLevelDataUpdated, OnUpdateLevelDataFailed);
        }

        private void RemoveBonusDetail(int index)
        {
            ShowProgress();

            BonusTable.DaliyPrizes.RemoveAt(index);
            BonusTable.DaliyPrizes.TrimExcess();

            UpdateBonusDetail(BonusTable);
        }

        private void OnLevelDataUpdated(SetTitleDataResult result)
        {
            HideProgress();
            GetBonusTable();
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
    }
}
#endif

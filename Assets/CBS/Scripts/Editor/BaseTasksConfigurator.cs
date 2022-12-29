#if ENABLE_PLAYFABADMIN_API
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.AdminModels;
using UnityEditor;
using System.Linq;
using System;
using CBS.Scriptable;
using CBS.Editor.Window;
using CBS.Core;

namespace CBS.Editor
{
    public class BaseTasksConfigurator<TTask, TTaskData, TWindow> : BaseConfigurator where TTask : CBSTask, new() where TTaskData : CBSTasksData<TTask>, new() where TWindow : AddTaskWindow<TTask>
    {
        protected virtual string TASK_TITLE_ID => CBSConstants.AchievementsTitleKey;

        protected override string Title => "Tasks";

        protected override bool DrawScrollView => true;

        protected virtual string[] Titles => new string[] { "Tasks", "Additional configs"};

        protected virtual string ItemKey => "task";

        private EditorData EditorData { get; set; }

        private List<PlayFab.AdminModels.CatalogItem> CachedItems { get; set; }
        private Categories CachedItemCategories { get; set; }
        private Categories CachedLootBoxCategories { get; set; }
        private List<string> CacheCurrencies { get; set; }

        protected TTaskData TasksData { get; set; }

        private int SelectedToolBar { get; set; }

        private GUILayoutOption[] AddButtonOptions
        {
            get
            {
                return new GUILayoutOption[] { GUILayout.Height(30), GUILayout.Width(150) };
            }
        }

        private TasksIcons Icons { get; set; }

        public override void Init(MenuTitles title)
        {
            base.Init(title);
            EditorData = CBSScriptable.Get<EditorData>();
            Icons = CBSScriptable.Get<TasksIcons>();
            GetTaskTable();
        }

        protected override void OnDrawInside()
        {
            SelectedToolBar = GUILayout.Toolbar(SelectedToolBar, Titles);

            switch (SelectedToolBar)
            {
                case 0:
                    DrawTasks();
                    break;
                case 1:
                    DrawConfigs();
                    break;
                default:
                    break;
            }
        }

        private void DrawTasks()
        {
            GUILayout.Space(20);

            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 14;

            var middleStyle = new GUIStyle(GUI.skin.label);
            middleStyle.fontStyle = FontStyle.Bold;
            middleStyle.fontSize = 14;
            middleStyle.alignment = TextAnchor.MiddleCenter;

            // draw add queue 
            GUILayout.BeginHorizontal();

            // draw name
            GUILayout.Space(27);
            EditorGUILayout.LabelField("Name", titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(200) });

            // draw count
            GUILayout.Space(155);
            EditorGUILayout.LabelField("Steps", titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(200) });

            // draw mode
            GUILayout.Space(40);
            EditorGUILayout.LabelField("Level", titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(100) });

            GUILayout.FlexibleSpace();
            if (EditorUtils.DrawButton("Add new "+ ItemKey, EditorData.AddColor, 12, AddButtonOptions))
            {
                AddTaskWindow<TTask>.Show<TWindow>(new TTask(), ItemAction.ADD, onAdd => {
                    AddNewTask(onAdd);
                });
                GUIUtility.ExitGUI();
            }
            GUILayout.EndHorizontal();

            EditorUtils.DrawUILine(Color.black, 2, 20);

            GUILayout.Space(20);

            if (TasksData == null)
                return;

            var taskList = TasksData.GetTasks();

            for (int i = 0; i < taskList.Count; i++)
            {
                var task = taskList[i];
                GUILayout.BeginHorizontal();
                string positionString = (i + 1).ToString();
                var positionDetail = task;

                EditorGUILayout.LabelField(positionString, titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(20), GUILayout.Height(50) });

                var actvieSprite = Icons.GetSprite(task.ID);
                var iconTexture = actvieSprite == null ? null : actvieSprite.texture;
                GUILayout.Button(iconTexture, GUILayout.Width(50), GUILayout.Height(50));

                // draw title
                EditorGUILayout.LabelField(task.Title, titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(200), GUILayout.Height(50) });

                // draw steps
                var stepsLabel = task.Type == TaskType.ONE_SHOT ? "One shot" : task.Steps.ToString();
                GUILayout.Space(90);
                EditorGUILayout.LabelField(stepsLabel.ToString(), middleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(70), GUILayout.Height(50) });

                // draw level
                var levelLabel = task.IsLockedByLevel ? task.LockLevel.ToString() : "--";
                GUILayout.Space(188);
                EditorGUILayout.LabelField(levelLabel.ToString(), titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(50), GUILayout.Height(50) });

                GUILayout.FlexibleSpace();

                GUILayout.Space(50);
                // draw edit button
                if (EditorUtils.DrawButton("Edit", EditorData.EditColor, 12, new GUILayoutOption[] { GUILayout.MaxWidth(80), GUILayout.Height(50) }))
                {
                    AddTaskWindow<TTask>.Show<TWindow>(task, ItemAction.EDIT, onEdit => {
                        SaveSaveTasksTable(TasksData);
                    });
                    GUIUtility.ExitGUI();
                }

                // draw prize button
                if (EditorUtils.DrawButton("Prize", EditorData.AddPrizeColor, 12, new GUILayoutOption[] { GUILayout.MaxWidth(80), GUILayout.Height(50) }))
                {
                    var prize = task.Reward ?? new PrizeObject();
                    ShowPrizeDialog(prize, true, result => {
                        task.Reward = prize;
                        SaveSaveTasksTable(TasksData);
                    });
                }

                // draw remove button
                if (EditorUtils.DrawButton("Remove", EditorData.RemoveColor, 12, new GUILayoutOption[] { GUILayout.MaxWidth(60), GUILayout.Height(50) }))
                {
                    string questionsText = string.Format("Are you sure you want to remove this {0}?", ItemKey);
                    int option = EditorUtility.DisplayDialogComplex("Warning",
                        questionsText,
                        "Yes",
                        "No",
                        string.Empty);
                    switch (option)
                    {
                        // ok.
                        case 0:
                            RemoveTask(task);
                            break;
                    }
                }

                GUILayout.EndHorizontal();

                EditorUtils.DrawUILine(Color.grey, 1, 20);

                GUILayout.Space(10);
            }
        }

        protected virtual void DrawConfigs()
        {
            GUILayout.Space(5);
            EditorUtils.DrawUILine(Color.black, 2, 20);
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();

            TasksData.AutomaticReward = EditorGUILayout.Toggle("Automatic reward", TasksData.AutomaticReward);
            
            GUILayout.FlexibleSpace();

            if (EditorUtils.DrawButton("Save", EditorData.SaveColor, 12, new GUILayoutOption[] { GUILayout.MaxWidth(80), GUILayout.Height(30) }))
            {
                SaveSaveTasksTable(TasksData);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            var autoRewardHelpText = string.Format("Enable this option to automatically reward the player after completing {0}. If this option is disabled, you will need to call additional API methods to receive reward", ItemKey);
            EditorGUILayout.HelpBox(autoRewardHelpText, MessageType.Info);
        }

        // get level table
        private void GetTaskTable()
        {
            var keyList = new List<string>();
            keyList.Add(TASK_TITLE_ID);
            var dataRequest = new GetTitleDataRequest
            {
                Keys = keyList
            };

            PlayFabAdminAPI.GetTitleData(dataRequest, OnTasksTableGetted, OnTaskTableError);
        }

        private void OnTasksTableGetted(GetTitleDataResult result)
        {
            bool tableExist = result.Data.ContainsKey(TASK_TITLE_ID);
            if (tableExist)
            {
                string tableRaw = result.Data[TASK_TITLE_ID];

                Debug.Log(tableRaw);
                var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                var table = jsonPlugin.DeserializeObject<TTaskData>(tableRaw);
                if (table.GetTasks() == null)
                {
                    table.NewInstance();
                }
                HideProgress();
                TasksData = table;
            }
            else
            {
                TasksData = new TTaskData();
                TasksData.NewInstance();
            }
            HideProgress();
        }

        private void OnTaskTableError(PlayFabError error)
        {
            HideProgress();
            AddErrorLog(error);
        }

        // add empty achievements
        private void AddNewTask(TTask task)
        {
            TasksData.Add(task);
            SaveSaveTasksTable(TasksData);
        }

        private void SaveSaveTasksTable(TTaskData data)
        {
            ShowProgress();
            var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
            string listRaw = jsonPlugin.SerializeObject(data);
            Debug.Log("list raw = "+ listRaw);

            var dataRequest = new SetTitleDataRequest
            {
                Key = TASK_TITLE_ID,
                Value = listRaw
            };

            PlayFabAdminAPI.SetTitleData(dataRequest, OnTasksTableSaved, OnSaveAchievementsFailed);
        }

        private void RemoveTask(TTask achievement)
        {
            TasksData.Remove(achievement);
            ShowProgress();

            SaveSaveTasksTable(TasksData);
        }

        private void OnTasksTableSaved(SetTitleDataResult result)
        {
            HideProgress();
            GetTaskTable();
        }

        private void OnSaveAchievementsFailed(PlayFabError error)
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

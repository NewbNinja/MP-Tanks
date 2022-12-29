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
using CBS.Scriptable;
using PlayFab.MultiplayerModels;
using PlayFab.ClientModels;
using CBS.Editor.Window;

namespace CBS.Editor
{
    public class LeaderboardsConfigurator : BaseConfigurator
    {
        protected override string Title => "Leaderboards Configuration";

        private List<PlayerStatisticDefinition> LeaderboardsList { get; set; }

        protected override bool DrawScrollView => true;

        private EditorData EditorData { get; set; }

        private GUILayoutOption[] AddButtonOptions
        {
            get
            {
                return new GUILayoutOption[] { GUILayout.Height(30), GUILayout.Width(140) };
            }
        }

        private string[] LockedStatistics = new string[] { 
            CBSConstants.ClanStatisticKey,
            CBSConstants.StaticsticExpKey
        };

        public override void Init(MenuTitles title)
        {
            base.Init(title);
            EditorData = CBSScriptable.Get<EditorData>();

            GetLeaderboardsList();
        }

        protected override void OnDrawInside()
        {
            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 14;

            // draw add queue 

            GUILayout.BeginHorizontal();

            // draw name
            GUILayout.Space(27);
            EditorGUILayout.LabelField("Statistics Name", titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(200) });

            // draw count
            GUILayout.Space(120);
            EditorGUILayout.LabelField("Aggregation", titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(200) });

            // draw mode
            GUILayout.Space(60);
            EditorGUILayout.LabelField("Reset Interval", titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(200) });

            GUILayout.FlexibleSpace();
            if (EditorUtils.DrawButton("Add new leaderboard", EditorData.AddColor, 12, AddButtonOptions))
            {
                AddStatisticsWindow.Show(updatedStatistics => {
                    AddNewStatistics(updatedStatistics);
                }, new PlayerStatisticDefinition(), ItemAction.ADD);
                GUIUtility.ExitGUI();
            }
            GUILayout.EndHorizontal();

            EditorUtils.DrawUILine(Color.black, 2, 20);

            GUILayout.Space(20);

            if (LeaderboardsList != null && LeaderboardsList.Count > 0)
            {
                for (int i = 0; i < LeaderboardsList.Count; i++)
                {
                    var leaderboard = LeaderboardsList[i];
                    GUILayout.BeginHorizontal();
                    string positionString = (i + 1).ToString();
                    var positionDetail = leaderboard;
                    var isLocked = LockedStatistics.Contains(leaderboard.StatisticName);

                    EditorGUILayout.LabelField(positionString, titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(20), GUILayout.Height(50) });

                    var matchTexture = ResourcesUtils.GetLeaderboardImage();
                    GUILayout.Button(matchTexture, GUILayout.Width(50), GUILayout.Height(50));

                    EditorGUILayout.LabelField(leaderboard.StatisticName, titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(200), GUILayout.Height(50) });

                    // draw aggregation
                    var playerCount = leaderboard.AggregationMethod ?? StatisticAggregationMethod.Last;
                    GUILayout.Space(90);
                    EditorGUILayout.LabelField(playerCount.ToString(), titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(50), GUILayout.Height(50) });

                    // draw reset interval
                    var mode = leaderboard.VersionChangeInterval ?? StatisticResetIntervalOption.Never;
                    GUILayout.Space(210);
                    EditorGUILayout.LabelField(mode.ToString(), titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(50), GUILayout.Height(50) });

                    GUILayout.FlexibleSpace();

                    EditorGUI.BeginDisabledGroup(isLocked);
                    GUILayout.Space(50);
                    if (EditorUtils.DrawButton("Configurate", EditorData.AddPrizeColor, 12, new GUILayoutOption[] { GUILayout.MaxWidth(150), GUILayout.Height(50) }))
                    {
                        AddStatisticsWindow.Show(updatedStatistics => {
                            SaveStatistics(updatedStatistics);
                        }, leaderboard, ItemAction.EDIT);
                        GUIUtility.ExitGUI();
                    }
                    EditorGUI.EndDisabledGroup();

                    // draw save button
                    if (EditorUtils.DrawButton("Reset", EditorData.RemoveColor, 12, new GUILayoutOption[] { GUILayout.MaxWidth(100), GUILayout.Height(50) }))
                    {
                        int option = EditorUtility.DisplayDialogComplex("Warning",
                            "Are you sure you want to reset this leaderboard?",
                            "Yes",
                            "No",
                            string.Empty);
                        switch (option)
                        {
                            // ok.
                            case 0:
                                ResetLeaderboard(leaderboard);
                                break;
                        }
                    }

                    GUILayout.EndHorizontal();

                    EditorUtils.DrawUILine(Color.grey, 1, 20);

                    GUILayout.Space(10);
                }
            }
        }

        private void GetLeaderboardsList()
        {
            ShowProgress();

            var request = new GetPlayerStatisticDefinitionsRequest { };

            PlayFabAdminAPI.GetPlayerStatisticDefinitions(request, onGet => {
                var statistics = onGet.Statistics;
                OnGetLeaderboardsList(statistics);
            }, onFailed => {
                OnGetLeaderboardsListFailed(onFailed);
            });
        }

        // get statistics
        private void OnGetLeaderboardsList(List<PlayerStatisticDefinition> list)
        {
            HideProgress();
            LeaderboardsList = list;
        }

        private void OnGetLeaderboardsListFailed(PlayFabError error)
        {
            AddErrorLog(error);
            HideProgress();
        }

        // add new leaderboard
        private void AddNewStatistics(PlayerStatisticDefinition statisticDefinition)
        {
            ShowProgress();
            var request = new CreatePlayerStatisticDefinitionRequest {
                StatisticName = statisticDefinition.StatisticName,
                AggregationMethod = statisticDefinition.AggregationMethod,
                VersionChangeInterval = statisticDefinition.VersionChangeInterval
            };
            PlayFabAdminAPI.CreatePlayerStatisticDefinition(request, onCreate => {
                HideProgress();
                GetLeaderboardsList();
            }, onFailed => {
                AddErrorLog(onFailed);
                HideProgress();
            });
        }

        private void SaveStatistics(PlayerStatisticDefinition statisticDefinition)
        {
            ShowProgress();
            var request = new UpdatePlayerStatisticDefinitionRequest { 
                StatisticName = statisticDefinition.StatisticName,
                AggregationMethod = statisticDefinition.AggregationMethod,
                VersionChangeInterval = statisticDefinition.VersionChangeInterval
            };

            PlayFabAdminAPI.UpdatePlayerStatisticDefinition(request, onUpdate=> {
                HideProgress();
                GetLeaderboardsList();
            }, onFailed=>{
                AddErrorLog(onFailed);
                HideProgress();
            });
        }

        private void OnUpdateQueue(SetMatchmakingQueueResult result)
        {
            Debug.Log("OnUpdateQueue " + result.ToJson());
            HideProgress();
            GetLeaderboardsList();
        }

        private void OnUpdateQueueFailed(PlayFabError error)
        {
            AddErrorLog(error);
            HideProgress();
        }

        // remove queues
        private void ResetLeaderboard(PlayerStatisticDefinition statistic)
        {
            ShowProgress();

            string statisticName = statistic.StatisticName;
            var request = new IncrementPlayerStatisticVersionRequest { 
                StatisticName = statisticName
            };

            PlayFabAdminAPI.IncrementPlayerStatisticVersion(request, onReset => {
                HideProgress();
                GetLeaderboardsList();
            }, onFailed => {
                HideProgress();
                AddErrorLog(onFailed);
            });
        }
    }
}
#endif

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
    public class MatchmakingConfigurator : BaseConfigurator
    {
        protected override string Title => "Matchmaking Configuration";

        private List<MatchmakingQueueConfig> MatchmakingQueues { get; set; }

        private string PlayFabID { get; set; }

        private string TitleEntityToken;

        private IFabMatchmaking FabMatchmaking { get; set; }


        protected override bool DrawScrollView => true;

        private EditorData EditorData { get; set; }

        private GUILayoutOption[] AddButtonOptions
        {
            get
            {
                return new GUILayoutOption[] { GUILayout.Height(30), GUILayout.Width(140) };
            }
        }

        public override void Init(MenuTitles title)
        {
            base.Init(title);
            FabMatchmaking = FabExecuter.Get<FabMatchmaking>();
            EditorData = CBSScriptable.Get<EditorData>();

            PlayFabClientAPI.ForgetAllCredentials();
            if (PlayFabClientAPI.IsClientLoggedIn())
            {
                GetMatchmakingList();
            }
            else
            {
                AuthPlayer();
            }
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
            EditorGUILayout.LabelField("Name", titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(200) });

            // draw count
            GUILayout.Space(120);
            EditorGUILayout.LabelField("Players", titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(200) });

            // draw mode
            GUILayout.Space(60);
            EditorGUILayout.LabelField("Mode", titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(200) });

            GUILayout.FlexibleSpace();
            if (EditorUtils.DrawButton("Add new queue", EditorData.AddColor, 12, AddButtonOptions))
            {
                AddMatchmakingQueueWindow.Show(newItem => {
                    AddNewQueue(newItem);
                }, new MatchmakingQueueConfig(), ItemAction.ADD);
                GUIUtility.ExitGUI();
            }
            GUILayout.EndHorizontal();

            EditorUtils.DrawUILine(Color.black, 2, 20);

            GUILayout.Space(20);

            if (MatchmakingQueues == null || MatchmakingQueues.Count == 0)
            {
                if (EditorUtils.DrawButton("Import default", EditorData.AddPrizeColor, 12, new GUILayoutOption[] { GUILayout.MaxWidth(150), GUILayout.Height(30) }))
                {
                    ImportDefaultQueues();
                }
            }

            if (MatchmakingQueues != null && MatchmakingQueues.Count > 0)
            {
                for (int i = 0; i < MatchmakingQueues.Count; i++)
                {
                    var queue = MatchmakingQueues[i];
                    GUILayout.BeginHorizontal();
                    string positionString = (i + 1).ToString();
                    var positionDetail = queue;

                    EditorGUILayout.LabelField(positionString, titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(20), GUILayout.Height(50) });

                    var matchTexture = ResourcesUtils.GetMatchImage();
                    GUILayout.Button(matchTexture, GUILayout.Width(50), GUILayout.Height(50));

                    EditorGUILayout.LabelField(queue.Name, titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(200), GUILayout.Height(50) });

                    // draw count
                    var playerCount = queue.MaxMatchSize;
                    GUILayout.Space(90);
                    EditorGUILayout.LabelField(playerCount.ToString(), titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(50), GUILayout.Height(50) });

                    // draw mode
                    var mode = queue.Teams == null || queue.Teams.Count == 0 ? MatchmakingMode.Single : MatchmakingMode.Team;
                    GUILayout.Space(188);
                    EditorGUILayout.LabelField(mode.ToString(), titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(50), GUILayout.Height(50) });

                    GUILayout.FlexibleSpace();

                    GUILayout.Space(50);
                    if (EditorUtils.DrawButton("Configurate", EditorData.AddPrizeColor, 12, new GUILayoutOption[] { GUILayout.MaxWidth(150), GUILayout.Height(50) }))
                    {
                        AddMatchmakingQueueWindow.Show(newItem => {
                            AddNewQueue(newItem);
                        }, queue, ItemAction.EDIT);
                        GUIUtility.ExitGUI();
                    }

                    // draw save button
                    if (EditorUtils.DrawButton("Remove", EditorData.RemoveColor, 12, new GUILayoutOption[] { GUILayout.MaxWidth(100), GUILayout.Height(50) }))
                    {
                        int option = EditorUtility.DisplayDialogComplex("Warning",
                            "Are you sure you want to remove this queue?",
                            "Yes",
                            "No",
                            string.Empty);
                        switch (option)
                        {
                            // ok.
                            case 0:
                                RemoveQueue(queue);
                                break;
                        }
                    }

                    GUILayout.EndHorizontal();

                    EditorUtils.DrawUILine(Color.grey, 1, 20);

                    GUILayout.Space(10);
                }
            }
        }

        private void AuthPlayer()
        {
            ShowProgress();
            var request = new LoginWithCustomIDRequest {
                CreateAccount = true,
                CustomId = SystemInfo.deviceUniqueIdentifier
            };
            
            PlayFabClientAPI.LoginWithCustomID(request, onLogin => {
                PlayFabID = onLogin.PlayFabId;
                GetEntityToken(GetMatchmakingList);
            }, onFailed => {
                OnGetMatchmakingListFailed(onFailed);
            });
        }

        private void GetEntityToken(Action onGet)
        {
            var request = new PlayFab.AuthenticationModels.GetEntityTokenRequest();

            TitleEntityToken = null;

            PlayFabAuthenticationAPI.GetEntityToken(
                request,
                result =>
                {
                    Debug.Log("On Get Entity Token "+ result.EntityToken);
                    TitleEntityToken = result.EntityToken;
                    onGet?.Invoke();
                },
                error =>
                {
                    AddErrorLog(error);
                    HideProgress();
                }
            );
        }

        private void GetMatchmakingList()
        {
            ShowProgress();
            FabMatchmaking.GetMatchmakingList(onGet => { 
                if (onGet.Error != null)
                {
                    Debug.Log(onGet.Error.Message);
                    OnGetMatchmakingListFailed(null);
                }
                else
                {
                    Debug.Log(onGet.FunctionResult);
                    var rawResult = onGet.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var result = jsonPlugin.DeserializeObject<ListMatchmakingQueuesResult>(rawResult);
                    OnGetMatchmakingList(result);
                }
            }, onFailed => {
                OnGetMatchmakingListFailed(onFailed);
            });
        }

        // get queues
        private void OnGetMatchmakingList(ListMatchmakingQueuesResult result)
        {
            HideProgress();
            MatchmakingQueues = result.MatchMakingQueues;
            Debug.Log("MatchmakingQueues Count = "+ MatchmakingQueues.Count);
        }

        private void OnGetMatchmakingListFailed(PlayFabError error)
        {
            AddErrorLog(error);
            HideProgress();
        }

        // add new queues
        private void AddNewQueue(MatchmakingQueueConfig queueConfig)
        {
            ShowProgress();
            Debug.Log(JsonUtility.ToJson(queueConfig));
            FabMatchmaking.UpdateMatchmakingQueue(queueConfig, onUpdate => {
                if (onUpdate.Error != null)
                {
                    Debug.Log(onUpdate.Error.ToJson());
                    OnUpdateQueueFailed(null);
                }
                else
                {
                    var rawResult = onUpdate.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var result = jsonPlugin.DeserializeObject<SetMatchmakingQueueResult>(rawResult);

                    OnUpdateQueue(result);
                }
            }, onFailed => {
                OnUpdateQueueFailed(onFailed);
            });
        }

        private void OnUpdateQueue(SetMatchmakingQueueResult result)
        {
            Debug.Log("OnUpdateQueue " + result.ToJson());
            HideProgress();
            GetMatchmakingList();
        }

        private void OnUpdateQueueFailed(PlayFabError error)
        {
            AddErrorLog(error);
            HideProgress();
        }

        // remove queues
        private void RemoveQueue(MatchmakingQueueConfig queueConfig)
        {
            ShowProgress();

            string queueName = queueConfig.Name;

            FabMatchmaking.RemoveMatchmakingQueue(queueName, onRemove => { 
                if (onRemove.Error != null)
                {
                    HideProgress();
                    AddErrorLog(onRemove.Error.Message);
                }
                else
                {
                    HideProgress();
                    GetMatchmakingList();
                }
            }, onFailed => {
                HideProgress();
                AddErrorLog(onFailed);
            });
        }

        // import default
        private void ImportDefaultQueues()
        {
            var singleQueues = new MatchmakingQueueConfig {
                Name = "SingleGame",
                MaxMatchSize = 2,
                MinMatchSize = 2
            };

            var deathMatchQueues = new MatchmakingQueueConfig
            {
                Name = "DeathMatch",
                MaxMatchSize = 4,
                MinMatchSize = 4
            };

            var teamQueues = new MatchmakingQueueConfig
            {
                Name = "TeamGame",
                MaxMatchSize = 10,
                MinMatchSize = 10,
                Teams = new List<MatchmakingQueueTeam>() { 
                    new MatchmakingQueueTeam{
                        Name = "Blue",
                        MaxTeamSize = 5,
                        MinTeamSize = 5
                    },
                    new MatchmakingQueueTeam{
                        Name = "Red",
                        MaxTeamSize = 5,
                        MinTeamSize = 5
                    }
                }
            };

            AddNewQueue(singleQueues);
            AddNewQueue(teamQueues);
            AddNewQueue(deathMatchQueues);
        }
    }
}
#endif

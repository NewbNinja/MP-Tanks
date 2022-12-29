using CBS.Playfab;
using CBS.Utils;
using PlayFab;
using PlayFab.MultiplayerModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlayFab.QoS;
using RSG;
using UnityEngine;

namespace CBS
{
    public class CBSMatchmaking : CBSModule, IMatchmaking
    {
        /// <summary>
        /// Notifies about status change in Matchmaking
        /// </summary>
        public event Action<MatchmakingStatus> OnStatusChanged;

        /// <summary>
        /// Notifies about the successful completion of the search for opponents.
        /// </summary>
        public event Action<StartMatchResult> OnMatchStart;

        private IFabMatchmaking FabMatchmaking { get; set; }
        private IProfile Profile { get; set; }

        /// <summary>
        /// Current Queue name
        /// </summary>
        public string ActiveQueue { get; private set; }

        /// <summary>
        /// Current ticket id name
        /// </summary>
        public string ActiveTicketID { get; private set; }

        /// <summary>
        /// Active matchmaking status
        /// </summary>
        public MatchmakingStatus Status { get; private set; }

        private bool CompareIsRunning { get; set; }

        protected override void Init()
        {
            FabMatchmaking = FabExecuter.Get<FabMatchmaking>();
            Profile = Get<CBSProfile>();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                ClearInternalProcess();
            }
        }
#endif


        // API Methods

        /// <inheritdoc />
        public void FindMatchWithServerAllocation(FindMatchRequest request, Action<FindMatchResult> result)
        {
            var playFabQosService = new PlayFabQosApi();

            Promise<QosResult> qosResultPromise = playFabQosService.GetQosResultAsync();
            qosResultPromise.Then((qosResult =>
            {
                List<Latency> latencies = new List<Latency>();
                foreach (var qosResultRegionResult in qosResult.RegionResults)
                {
                    latencies.Add(new Latency
                    {
                        region = qosResultRegionResult.Region,
                        latency = qosResultRegionResult.LatencyMs
                    });
                }


                FindMatch(request, result, latencies.ToArray());
            }));
        }

        /// <summary>
        /// Creates a ticket to find opponents. After a successful call, listen for a change in the status of the queue.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        /// <param name="latencies"></param>
        public void FindMatch(FindMatchRequest request, Action<FindMatchResult> result, Latency[] latencies = null)
        {
            var entityId = Profile.EntityID;
            var profileID = Profile.PlayerID;
            var queueName = request.QueueName;
            var waitTime = request.WaitTime ?? CBSConstants.MatchmakingDefaultWaitTime;

            var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
            var playerLevel = Profile.CacheLevelInfo.CurrentLevel;

            // clear prev match is exist
            CancelMatch(queueName, onCancel =>
            {
                if (onCancel.IsSuccess)
                {
                    // get queue

                    GetMatchmakingQueue(queueName, onGetQueue =>
                    {
                        if (onGetQueue.IsSuccess)
                        {
                            var queue = onGetQueue.Queue;
                            int? nullableLevel = null;
                            double? nullableValue = null;


                            var dataObject = new CBSPlayerAttributes
                            {
                                ProfileID = profileID,
                                LevelEquality = queue.IsLevelEquality ? playerLevel.ToString() : string.Empty,
                                MatchmakingStringEquality =
                                    queue.IsStringEquality ? request.StringEqualityValue : string.Empty,
                                LevelDifference = queue.IsLevelDifference ? playerLevel : nullableLevel,
                                ValueDifference = queue.IsValueDifference ? request.DifferenceValue : nullableValue,
                                Latencies = latencies
                            };

                            var rawAttributes = jsonPlugin.SerializeObject(dataObject);

                            var playerAttributes = new MatchmakingPlayerAttributes
                            {
                                EscapedDataObject = rawAttributes
                            };

                            // create ticket
                            FabMatchmaking.CreateTicket(queueName, waitTime, entityId, playerAttributes, onCreate =>
                            {
                                var ticketID = onCreate.TicketId;
                                ActiveQueue = queueName;
                                ActiveTicketID = ticketID;

                                FabMatchmaking.GetMatchmakingTicket(queueName, ticketID, onGetTicket =>
                                {
                                    var date = onGetTicket.Created;
                                    var members = onGetTicket.Members;

                                    result?.Invoke(new FindMatchResult
                                    {
                                        IsSuccess = true,
                                        TicketID = ticketID,
                                        Queue = queue,
                                        CreatedDate = date
                                    });

                                    SetStatus(MatchmakingStatus.CreateTicket);

                                    StartRefreshTask();
                                }, OnFailedGetTicket =>
                                {
                                    // failed get ticket
                                    result?.Invoke(new FindMatchResult
                                    {
                                        IsSuccess = false,
                                        Error = SimpleError.FromTemplate(OnFailedGetTicket)
                                    });
                                });
                            }, onFailed =>
                            {
                                // failed create ticket
                                result?.Invoke(new FindMatchResult
                                {
                                    IsSuccess = false,
                                    Error = SimpleError.FromTemplate(onFailed)
                                });
                            });
                        }
                        else
                        {
                            // failed get queue
                            result?.Invoke(new FindMatchResult
                            {
                                IsSuccess = false,
                                Error = onGetQueue.Error
                            });
                        }
                    });
                }
                else
                {
                    // failed clear tickets
                    result?.Invoke(new FindMatchResult
                    {
                        IsSuccess = false,
                        Error = onCancel.Error
                    });
                }
            });
        }


        /// <summary>
        /// Get a detailed description of the queue by name.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="result"></param>
        public void GetMatchmakingQueue(string queueName, Action<GetQueueResult> result)
        {
            FabMatchmaking.GetQueue(queueName, onGet =>
            {
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetQueueResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawResult = onGet.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var fabResult = jsonPlugin.DeserializeObject<GetMatchmakingQueueResult>(rawResult);
                    var fabQueue = fabResult.MatchmakingQueue;
                    var cbsQueue = CBSMatchmakingQueue.FromMatchConfig(fabQueue);

                    result?.Invoke(new GetQueueResult
                    {
                        IsSuccess = true,
                        Queue = cbsQueue
                    });
                }
            }, onFailed =>
            {
                result?.Invoke(new GetQueueResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Get a list of all Queues on the server.
        /// </summary>
        /// <param name="result"></param>
        public void GetMatchmakingQueuesList(Action<GetMatchmakingListResult> result)
        {
            FabMatchmaking.GetMatchmakingList(onGet =>
            {
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetMatchmakingListResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawResult = onGet.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var fabResult = jsonPlugin.DeserializeObject<ListMatchmakingQueuesResult>(rawResult);
                    var fabQueuesResult = fabResult.MatchMakingQueues;
                    var cbsQueues = new List<CBSMatchmakingQueue>();

                    foreach (var fabQueue in fabQueuesResult)
                    {
                        var queueMode = fabQueue.Teams == null || fabQueue.Teams.Count == 0
                            ? MatchmakingMode.Single
                            : MatchmakingMode.Team;
                        var newQueue = CBSMatchmakingQueue.FromMatchConfig(fabQueue);
                        cbsQueues.Add(newQueue);
                    }

                    result?.Invoke(new GetMatchmakingListResult
                    {
                        IsSuccess = true,
                        Queues = cbsQueues
                    });
                }
            }, onFailed =>
            {
                result?.Invoke(new GetMatchmakingListResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Cancels all search tickets for the current player.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="result"></param>
        public void CancelMatch(string queueName, Action<CancelMatchResult> result)
        {
            var entityID = Profile.EntityID;
            FabMatchmaking.CancelMatctForPlayer(queueName, entityID, onCancel =>
            {
                result?.Invoke(new CancelMatchResult
                {
                    IsSuccess = true
                });
                if (Status != MatchmakingStatus.None)
                {
                    SetStatus(MatchmakingStatus.Canceled);
                }

                ClearInternalProcess();
            }, onFailed =>
            {
                result?.Invoke(new CancelMatchResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        // internal
        private void StartRefreshTask()
        {
            CompareIsRunning = true;
            CompareWorkTask();
        }

        private void StopRefreshTask()
        {
            CompareIsRunning = false;
        }

        private async void CompareWorkTask()
        {
#if UNITY_WEBGL
            float startTime = Time.time;
            while (Time.time < startTime + CBSConstants.MatchmakingRefreshTime/1000f) await Task.Yield();
#else
            await Task.Delay(CBSConstants.MatchmakingRefreshTime);
#endif
            if (!CompareIsRunning)
            {
                return;
            }

            FabMatchmaking.GetMatchmakingTicket(ActiveQueue, ActiveTicketID, onGet =>
            {
                var status = onGet.Status;
                var members = onGet.Members;
                Debug.Log(status);
                Debug.Log("Match ID " + onGet.MatchId);
                switch (status)
                {
                    case MatchmakingUtils.StatusCanceled:
                        SetStatus(MatchmakingStatus.Canceled);
                        ClearInternalProcess();
                        break;
                    case MatchmakingUtils.StatusMatched:
                        StartMatch(onGet);
                        StopRefreshTask();
                        break;
                    case MatchmakingUtils.StatusWaitingForMatch:
                        SetStatus(MatchmakingStatus.WaitingForMatch);
                        break;
                    case MatchmakingUtils.StatusWaitingForPlayers:
                        SetStatus(MatchmakingStatus.WaitingForPlayers);
                        break;
                }

                CompareWorkTask();
            }, onError =>
            {
                Debug.Log(onError.ErrorMessage);
                CompareWorkTask();
            });
        }

        private void SetStatus(MatchmakingStatus status)
        {
            if (Status != status)
            {
                OnStatusChanged?.Invoke(status);
            }

            Status = status;
        }

        private void StartMatch(GetMatchmakingTicketResult result)
        {
            var matchID = result.MatchId;
            var queueName = result.QueueName;

            FabMatchmaking.GetMatch(queueName, matchID, onGet =>
            {
                Debug.Log("Start Match with ID = " + onGet.MatchId + " with players " + onGet.Members.Count);

                var members = onGet.Members;
                var players = members.Select(x => CBSMatchmakingPlayer.FromFabModel(x)).ToList();
                SetStatus(MatchmakingStatus.Matched);

                var matchResult = new StartMatchResult
                {
                    MatchID = onGet.MatchId,
                    TicketID = ActiveTicketID,
                    QueueName = queueName,
                    Players = players,
                    ServerDetails = onGet.ServerDetails
                };

                OnMatchStart?.Invoke(matchResult);
            }, onFailed => { CancelMatch(queueName, onCancel => { ClearInternalProcess(); }); });
        }

        protected override void OnLogout()
        {
            base.OnLogout();
            ClearInternalProcess();
        }

        private void ClearInternalProcess()
        {
            ActiveQueue = string.Empty;
            ActiveTicketID = string.Empty;

            StopRefreshTask();

            SetStatus(MatchmakingStatus.None);
        }
    }

    public enum MatchmakingMode
    {
        Single,
        Team
    }

    public enum MatchmakingStatus
    {
        None,
        CreateTicket,
        WaitingForPlayers,
        WaitingForMatch,
        Canceled,
        Matched
    }

    [Serializable]
    public class CBSPlayerAttributes
    {
        public string ProfileID;
        public string LevelEquality;
        public string MatchmakingStringEquality;
        public double? LevelDifference;
        public double? ValueDifference;
        public Latency[] Latencies;
    }

    public class Latency
    {
        public string region;
        public int latency;
    }

    [Serializable]
    public class CBSMatchmakingQueue
    {
        public string QueueName;
        public MatchmakingMode Mode;
        public int PlayersCount;

        public bool IsDuel() => PlayersCount == 2;
        public bool IsLevelEquality { get; private set; }
        public bool IsStringEquality { get; private set; }
        public bool IsLevelDifference { get; private set; }
        public bool IsValueDifference { get; private set; }

        public static CBSMatchmakingQueue FromMatchConfig(MatchmakingQueueConfig config)
        {
            var queueMode = config.Teams == null || config.Teams.Count == 0
                ? MatchmakingMode.Single
                : MatchmakingMode.Team;
            return new CBSMatchmakingQueue
            {
                Mode = queueMode,
                QueueName = config.Name,
                PlayersCount = (int)config.MaxMatchSize,
                IsLevelEquality = config.StringEqualityRules == null
                    ? false
                    : config.StringEqualityRules.Any(x => x.Name == CBSConstants.LevelEqualityRuleName),
                IsStringEquality = config.StringEqualityRules == null
                    ? false
                    : config.StringEqualityRules.Any(x => x.Name == CBSConstants.StringEqualityRuleName),
                IsLevelDifference = config.DifferenceRules == null
                    ? false
                    : config.DifferenceRules.Any(x => x.Name == CBSConstants.LevelDifferenceRuleName),
                IsValueDifference = config.DifferenceRules == null
                    ? false
                    : config.DifferenceRules.Any(x => x.Name == CBSConstants.ValueDifferenceRuleName),
            };
        }
    }

    public class CBSMatchmakingPlayer
    {
        public string ProfileID;
        public string PlayerEntityID;
        public string Team;

        public static CBSMatchmakingPlayer FromFabModel(MatchmakingPlayerWithTeamAssignment fabPlayer)
        {
            var rawAttributes = fabPlayer.Attributes.DataObject != null
                ? fabPlayer.Attributes.DataObject.ToString()
                : fabPlayer.Attributes.EscapedDataObject;

            var attributes = JsonUtility.FromJson<CBSPlayerAttributes>(rawAttributes);

            return new CBSMatchmakingPlayer
            {
                ProfileID = attributes.ProfileID,
                PlayerEntityID = fabPlayer.Entity.Id,
                Team = fabPlayer.TeamId
            };
        }

        public static CBSMatchmakingPlayer FromFabModel(MatchmakingPlayer fabPlayer)
        {
            return new CBSMatchmakingPlayer
            {
                PlayerEntityID = fabPlayer.Entity.Id
            };
        }
    }

    public struct GetMatchmakingListResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public List<CBSMatchmakingQueue> Queues;
    }

    public struct FindMatchRequest
    {
        public string QueueName;
        public int? WaitTime;
        public string StringEqualityValue;
        public double DifferenceValue;
    }

    public struct FindMatchResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string TicketID;
        public DateTime CreatedDate;
        public CBSMatchmakingQueue Queue;
    }

    public struct GetQueueResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public CBSMatchmakingQueue Queue;
    }

    public struct CancelMatchResult
    {
        public bool IsSuccess;
        public SimpleError Error;
    }

    public struct StartMatchResult
    {
        public string MatchID;
        public string TicketID;
        public string QueueName;
        public List<CBSMatchmakingPlayer> Players;
        public ServerDetails ServerDetails;
    }
}
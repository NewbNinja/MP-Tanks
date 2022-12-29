using CBS.Playfab;
using PlayFab;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS
{
    public class CBSBattlePass : CBSModule, IBattlePass
    {
        private IFabBattlePass FabBattlePass { get; set; }
        private IProfile Profile { get; set; }

        /// <summary>
        /// Notify when experience has been gained for a specific Battle Pass.
        /// </summary>
        public event Action<string, int> OnExpirienceAdded;
        /// <summary>
        /// Notify when the reward was received.
        /// </summary>
        public event Action<PrizeObject> OnRewardRecived;
        /// <summary>
        /// Notify when premium access has been unlocked for a specific Battle Pass.
        /// </summary>
        public event Action<string> OnPremiumAccessGranted;

        protected override void Init()
        {
            FabBattlePass = FabExecuter.Get<FabBattlePass>();
            Profile = Get<CBSProfile>();
        }

        // Method API

        /// <summary>
        /// Get information about the state of the player's instances of Battle passes. Does not contain complete information about levels and rewards. More suitable for implementing badges.
        /// </summary>
        /// <param name="result"></param>
        public void GetPlayerStates(Action<GetPlayerBattlePassStatesResult> result)
        {
            GetUserStatesFromRequest(new BattlePassPlayerStateRequest {
                SpecificID = string.Empty,
                IncludeNotStarted = false,
                IncludeOutdated = false
            }, result);
        }

        /// <summary>
        /// Get information about the state of the player's instances of Battle passes. Does not contain complete information about levels and rewards. More suitable for implementing badges.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        public void GetPlayerStates(BattlePassPlayerStateRequest request, Action<GetPlayerBattlePassStatesResult> result)
        {
            GetUserStatesFromRequest(request, result);
        }

        /// <summary>
        /// Get complete information about the state of the player's instances of Battle passes and instance levels. Contains complete information about levels and rewards.
        /// </summary>
        /// <param name="battlePassID"></param>
        /// <param name="result"></param>
        public void GetBattlePassFullInformation(string battlePassID, Action<GetBattlePassFullInformationResult> result)
        {
            var profileID = Profile.PlayerID;
            FabBattlePass.GetBattlePassFullInformation(profileID, battlePassID, onGet =>
            {
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetBattlePassFullInformationResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawData = onGet.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var resultObject = jsonPlugin.DeserializeObject<BattlePassFullInformationCallback>(rawData);
                    result?.Invoke(new GetBattlePassFullInformationResult
                    {
                        IsSuccess = true,
                        PlayerState = resultObject.PlayerState,
                        Instance = resultObject.Instance
                    });
                }
            }, onFailed =>
            {
                result?.Invoke(new GetBattlePassFullInformationResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Add player experience for a specific instance of Battle Pass.
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="exp"></param>
        /// <param name="result"></param>
        public void AddExpirienceToInstance(string instanceID, int exp, Action<AddExpirienceToInstanceResult> result)
        {
            var profileID = Profile.PlayerID;
            FabBattlePass.AddExpirienceToInstance(profileID, exp, instanceID, onAdd => 
            {
                if (onAdd.Error != null)
                {
                    result?.Invoke(new AddExpirienceToInstanceResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onAdd.Error)
                    });
                }
                else
                {
                    var rawData = onAdd.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var resultObject = jsonPlugin.DeserializeObject<BattlePassAddExpirienceCallback>(rawData);
                    var resulteTable = resultObject.ExpTable;
                    resulteTable = resulteTable ?? new Dictionary<string, int>();
                    var defaultResult = resulteTable.FirstOrDefault();
                    OnExpirienceAdded?.Invoke(defaultResult.Key, defaultResult.Value);
    
                    result?.Invoke(new AddExpirienceToInstanceResult
                    {
                        IsSuccess = true,
                        InstanceID = resulteTable == null ? string.Empty : defaultResult.Key,
                        NewExp = resulteTable == null ? 0 : defaultResult.Value
                    });
                }
            }, onFailed => 
            {
                result?.Invoke(new AddExpirienceToInstanceResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Add player experience for all active instances of Battle Passes.
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="result"></param>
        public void AddExpirienceToAllActiveInstances(int exp, Action<AddExpirienceToAllInstancesResult> result)
        {
            var profileID = Profile.PlayerID;
            FabBattlePass.AddExpirienceToInstance(profileID, exp, string.Empty, onAdd =>
            {
                if (onAdd.Error != null)
                {
                    result?.Invoke(new AddExpirienceToAllInstancesResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onAdd.Error)
                    });
                }
                else
                {
                    var rawData = onAdd.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var resultObject = jsonPlugin.DeserializeObject<BattlePassAddExpirienceCallback>(rawData);
                    var resulteTable = resultObject.ExpTable;

                    foreach (var res in resulteTable)
                    {
                        OnExpirienceAdded?.Invoke(res.Key, res.Value);
                    }

                    result?.Invoke(new AddExpirienceToAllInstancesResult
                    {
                        IsSuccess = true,
                        NewExpTable = resulteTable
                    });
                }
            }, onFailed =>
            {
                result?.Invoke(new AddExpirienceToAllInstancesResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Grant the player a reward from a specific instance of Battle Pass.
        /// </summary>
        /// <param name="battlePassID"></param>
        /// <param name="level"></param>
        /// <param name="isPremium"></param>
        /// <param name="result"></param>
        public void GrantAwardToPlayer(string battlePassID, int level, bool isPremium, Action<GrandAwardToPlayerResult> result)
        {
            var profileID = Profile.PlayerID;
            FabBattlePass.GetRewardFromInstance(profileID, battlePassID, level, isPremium, onGrant => 
            {
                if (onGrant.Error != null)
                {
                    result?.Invoke(new GrandAwardToPlayerResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGrant.Error)
                    });
                }
                else
                {
                    var rawData = onGrant.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var resultObject = jsonPlugin.DeserializeObject<BattlePassGrantAwardCallback>(rawData);

                    var reward = resultObject.RecivedReward;
                    if (reward != null)
                    {
                        var currencies = reward.BundledVirtualCurrencies;
                        if (currencies != null)
                        {
                            var codes = currencies.Select(x => x.Key).ToArray();
                            Get<CBSCurrency>().ChangeRequest(codes);
                        }
                        OnRewardRecived?.Invoke(reward);
                    }

                    result?.Invoke(new GrandAwardToPlayerResult { 
                        IsSuccess = true,
                        InstanceID = resultObject.InstanceID,
                        IsPremium = resultObject.IsPremium,
                        RecivedReward = resultObject.RecivedReward
                    });
                }
            }, onFailed => 
            {
                result?.Invoke(new GrandAwardToPlayerResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Give the player premium access for a specific instance of Battle Pass.
        /// </summary>
        /// <param name="battlePassID"></param>
        /// <param name="result"></param>
        public void GrantPremiumAccessToPlayer(string battlePassID, Action<GrantPremiumAccessResult> result)
        {
            var profileID = Profile.PlayerID;
            FabBattlePass.GetPremiumAccess(profileID, battlePassID, onGrant => 
            { 
                if (onGrant.Error != null)
                {
                    result?.Invoke(new GrantPremiumAccessResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGrant.Error)
                    });
                }
                else
                {
                    OnPremiumAccessGranted?.Invoke(battlePassID);
                    result?.Invoke(new GrantPremiumAccessResult
                    {
                        IsSuccess = true,
                        InstanceID = battlePassID
                    });
                }
            }, onFailed => 
            {
                result?.Invoke(new GrantPremiumAccessResult {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Reset player data for a specific Battle Pass.
        /// </summary>
        /// <param name="battlePassID"></param>
        /// <param name="result"></param>
        public void ResetBattlePassPlayerState(string battlePassID, Action<ResetBattlePassStateResult> result)
        {
            var profileID = Profile.PlayerID;
            FabBattlePass.ResetInstanceForPlayer(profileID, battlePassID, onGrant =>
            {
                if (onGrant.Error != null)
                {
                    result?.Invoke(new ResetBattlePassStateResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGrant.Error)
                    });
                }
                else
                {
                    result?.Invoke(new ResetBattlePassStateResult
                    {
                        IsSuccess = true,
                        InstanceID = battlePassID
                    });
                }
            }, onFailed =>
            {
                result?.Invoke(new ResetBattlePassStateResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        // internal

        private void GetUserStatesFromRequest(BattlePassPlayerStateRequest request, Action<GetPlayerBattlePassStatesResult> result)
        {
            var profileID = Profile.PlayerID;
            var specificID = request.SpecificID;
            var includeNotStarted = request.IncludeNotStarted;
            var includeOutdated = request.IncludeOutdated;

            FabBattlePass.GetPlayerBattlePassStates(profileID, specificID, includeNotStarted, includeOutdated, onGet =>
            {
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetPlayerBattlePassStatesResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawData = onGet.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var resultObject = jsonPlugin.DeserializeObject<BattlePassPlayerStatesCallback>(rawData);
                    result?.Invoke(new GetPlayerBattlePassStatesResult
                    {
                        IsSuccess = true,
                        PlayerStates = resultObject.PlayerStates
                    });
                }
            }, onFailed =>
            {
                result?.Invoke(new GetPlayerBattlePassStatesResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }
    }

    public struct GetPlayerBattlePassStatesResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public List<BattlePassUserInfo> PlayerStates;
    }

    public struct GrandAwardToPlayerResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string InstanceID;
        public PrizeObject RecivedReward;
        public bool IsPremium;
    }

    public struct AddExpirienceToInstanceResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public int NewExp;
        public string InstanceID;
    }

    public struct GrantPremiumAccessResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string InstanceID;
    }

    public struct ResetBattlePassStateResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string InstanceID;
    }

    public struct AddExpirienceToAllInstancesResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public Dictionary<string, int> NewExpTable;
    }

    public struct GetBattlePassFullInformationResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public BattlePassUserInfo PlayerState;
        public BattlePassInstance Instance;

        public List<BattlePassLevelInfo> GetLevelTreeDetailList()
        {
            if (Instance == null || Instance.LevelTree == null)
                return new List<BattlePassLevelInfo>();
            var levelTree = Instance.LevelTree;
            var levelsInfo = new List<BattlePassLevelInfo>();

            var playerLevel = PlayerState.PlayerLevel;
            var collectedDefaultReward = PlayerState.CollectedSimpleReward;
            var collectedPremiumReward = PlayerState.CollectedPremiumReward;
            var levelsCount = levelTree.Count;
            for (int i=0;i< levelsCount; i++)
            {
                levelsInfo.Add(new BattlePassLevelInfo {
                    battlePassID = PlayerState.BattlePassID,
                    ExpStep = Instance.ExpStep,
                    IsPassActive = PlayerState.IsActive,
                    ExpOfCurrentLevel = PlayerState.ExpOfCurrentLevel,
                    IsDefaultRewardCollected = collectedDefaultReward == null ? false : collectedDefaultReward.Contains(i),
                    IsPremiumRewardCollected = collectedPremiumReward == null ? false : collectedPremiumReward.Contains(i),
                    IsPremium = PlayerState.PremiumRewardAvailable,
                    LevelDetail = levelTree[i],
                    LevelIndex = i,
                    PlayerLevel = PlayerState.PlayerLevel,
                    IsLast = levelsCount - 1 == i
                });
            }
            return levelsInfo;
        }
    }
}

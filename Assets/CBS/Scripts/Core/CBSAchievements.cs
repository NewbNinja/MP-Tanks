using CBS.Playfab;
using CBS.UI;
using PlayFab;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CBS.Core;

namespace CBS
{
    public class CBSAchievements : CBSModule, IAchievements
    {
        /// <summary>
        /// Notify when player complete achievement.
        /// </summary>
        public event Action<CompleteAchievementResult> OnCompleteAchievement;
        /// <summary>
        /// Notify when player receive reward for achievement.
        /// </summary>
        public event Action<PrizeObject> OnPlayerRewarded;

        private IProfile Profile { get; set; }
        private IFabAchievements FabAchievements { get; set; }

        protected override void Init()
        {
            Profile = Get<CBSProfile>();
            FabAchievements = FabExecuter.Get<FabAchievements>();
        }

        // API Methods

        /// <summary>
        /// Get information for all achievements and their player state.
        /// </summary>
        /// <param name="result"></param>
        public void GetAchievementsTable(Action<GetAchievementsTableResult> result)
        {
            InternalGetAchievements(AchievementsTabType.ALL, result);
        }

        /// <summary>
        /// Get information for all available achievements for player achievements
        /// </summary>
        /// <param name="result"></param>
        public void GetActiveAchievementsTable(Action<GetAchievementsTableResult> result)
        {
            InternalGetAchievements(AchievementsTabType.ACTIVE, result);
        }

        /// <summary>
        /// Get information for all completed achievements for player achievements
        /// </summary>
        /// <param name="result"></param>
        public void GetCompletedAchievementsTable(Action<GetAchievementsTableResult> result)
        {
            InternalGetAchievements(AchievementsTabType.COMPLETED, result);
        }

        /// <summary>
        /// Adds a point to an achievement. For Achievements "OneShot" completes it immediately, for Achievements "Steps" - adds one step
        /// </summary>
        /// <param name="achievementID"></param>
        /// <param name="result"></param>
        public void AddAchievementPoint(string achievementID, Action<ModifyAchievementPointResult> result)
        {
            InternalModifyPoints(achievementID, 1, ModifyMethod.ADD, result);
        }

        /// <summary>
        /// Adds the points you specified to the achievement. More suitable for Steps achievements.
        /// </summary>
        /// <param name="achievementID"></param>
        /// <param name="points"></param>
        /// <param name="result"></param>
        public void AddAchievementPoint(string achievementID, int points, Action<ModifyAchievementPointResult> result)
        {
            InternalModifyPoints(achievementID, points, ModifyMethod.ADD, result);
        }

        /// <summary>
        /// Updates the achievement points you specified. More suitable for Steps achievements.
        /// </summary>
        /// <param name="achievementID"></param>
        /// <param name="points"></param>
        /// <param name="result"></param>
        public void UpdateAchievementPoint(string achievementID, int points, Action<ModifyAchievementPointResult> result)
        {
            InternalModifyPoints(achievementID, points, ModifyMethod.UPDATE, result);
        }

        /// <summary>
        /// Pick up a reward from a completed achievement if it hasn't been picked up before.
        /// </summary>
        /// <param name="achievementID"></param>
        /// <param name="result"></param>
        public void PickupAchievementReward(string achievementID, Action<ModifyAchievementPointResult> result)
        {
            var profileID = Profile.PlayerID;

            FabAchievements.PickupReward(profileID, achievementID, onPick => { 
                if (onPick.Error != null)
                {
                    result?.Invoke(new ModifyAchievementPointResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onPick.Error)
                    });
                }
                else
                {
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var rawData = onPick.FunctionResult.ToString();
                    var resultObject = jsonPlugin.DeserializeObject<AddTaskPointCallbackData>(rawData);
                    var prize = resultObject.ReceivedReward;

                    if (resultObject != null && prize != null)
                    {
                        var currencies = prize.BundledVirtualCurrencies;
                        if (currencies != null)
                        {
                            var codes = currencies.Select(x => x.Key).ToArray();
                            Get<CBSCurrency>().ChangeRequest(codes);
                        }
                        OnPlayerRewarded?.Invoke(prize);
                    }

                    result?.Invoke(new ModifyAchievementPointResult
                    {
                        IsSuccess = true,
                        Achievement = resultObject.Task,
                        ReceivedReward = prize
                    });
                }
            }, onFailed => {
                result?.Invoke(new ModifyAchievementPointResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Resets the state of a player for a specific achievement.
        /// </summary>
        /// <param name="achievementID"></param>
        /// <param name="result"></param>
        public void ResetAchievement(string achievementID, Action<ResetAchievementResult> result)
        {
            var profileID = Profile.PlayerID;

            FabAchievements.ResetAchievement(profileID, achievementID, onReset => { 
                if (onReset.Error != null)
                {
                    result?.Invoke(new ResetAchievementResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onReset.Error)
                    });
                }
                else
                {
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var rawData = onReset.FunctionResult.ToString();
                    var resultObject = jsonPlugin.DeserializeObject<CBSTask>(rawData);

                    result?.Invoke(new ResetAchievementResult { 
                        IsSuccess = true,
                        Achievement = resultObject
                    });
                }
            }, onFailed => {
                result?.Invoke(new ResetAchievementResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        private void InternalModifyPoints(string achievementID, int points, ModifyMethod modify, Action<ModifyAchievementPointResult> result)
        {
            var profileID = Profile.PlayerID;

            FabAchievements.ModifyAchievementPoint(profileID, achievementID, points, modify, onAdd => {
                if (onAdd.Error != null)
                {
                    result?.Invoke(new ModifyAchievementPointResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onAdd.Error)
                    });
                }
                else
                {
                    Debug.Log(onAdd.FunctionResult);
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var rawData = onAdd.FunctionResult.ToString();
                    var resultObject = jsonPlugin.DeserializeObject<AddTaskPointCallbackData>(rawData);
                    var prize = resultObject.ReceivedReward;

                    if (resultObject != null && prize != null)
                    {
                        var currencies = prize.BundledVirtualCurrencies;
                        if (currencies != null)
                        {
                            var codes = currencies.Select(x => x.Key).ToArray();
                            Get<CBSCurrency>().ChangeRequest(codes);
                        }
                        OnPlayerRewarded?.Invoke(prize);
                    }

                    var complete = resultObject.Task.IsComplete;

                    if (complete)
                    {
                        OnCompleteAchievement?.Invoke(new CompleteAchievementResult { 
                            Achievement = resultObject.Task,
                            ReceivedReward = prize
                        });
                    }

                    result?.Invoke(new ModifyAchievementPointResult
                    {
                        IsSuccess = true,
                        Achievement = resultObject.Task,
                        ReceivedReward = prize
                    });
                }
            }, onFailed => {
                Debug.Log(onFailed);
                result?.Invoke(new ModifyAchievementPointResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        private void InternalGetAchievements(AchievementsTabType queryType, Action<GetAchievementsTableResult> result)
        {
            var profileID = Profile.PlayerID;

            FabAchievements.GetAchievementsTable(profileID, onGet => {
                if (onGet.Error == null)
                {
                    var rawData = onGet.FunctionResult.ToString();

                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var resultObject = jsonPlugin.DeserializeObject<AchievementsData>(rawData);
                    var achievementsList = resultObject.Achievements ?? new List<CBSTask>();

                    if (queryType == AchievementsTabType.ACTIVE)
                    {
                        achievementsList = achievementsList.Where(x => !x.IsComplete && x.IsAvailable).ToList();
                        resultObject.Achievements = achievementsList;
                    }
                    else if (queryType == AchievementsTabType.COMPLETED)
                    {
                        achievementsList = achievementsList.Where(x => x.IsComplete).ToList();
                        resultObject.Achievements = achievementsList;
                    }

                    result?.Invoke(new GetAchievementsTableResult
                    {
                        IsSuccess = true,
                        AchievementsData = resultObject
                    });
                }
                else
                {
                    result?.Invoke(new GetAchievementsTableResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
            }, onFailed => {
                result?.Invoke(new GetAchievementsTableResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }
    }

    public struct GetAchievementsTableResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public AchievementsData AchievementsData;
    }

    public struct ModifyAchievementPointResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public CBSTask Achievement;
        public PrizeObject ReceivedReward;
    }

    public struct ResetAchievementResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public CBSTask Achievement;
    }

    public struct CompleteAchievementResult
    {
        public CBSTask Achievement;
        public PrizeObject ReceivedReward;
    }
}

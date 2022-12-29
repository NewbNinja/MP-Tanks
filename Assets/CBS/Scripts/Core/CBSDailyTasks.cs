using CBS.Core;
using CBS.Playfab;
using PlayFab;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS
{
    public class CBSDailyTasks : CBSModule, IDailyTasks
    {
        /// <summary>
        /// Notify when player complete task.
        /// </summary>
        public event Action<CompleteTaskResult> OnCompleteTask;
        /// <summary>
        /// Notify when player reset tasks and get new
        /// </summary>
        public event Action<GetPlayerDailyTasksResult> OnTaskReseted;
        /// <summary>
        /// Notify when player receive reward for task.
        /// </summary>
        public event Action<PrizeObject> OnPlayerRewarded;

        private IProfile Profile { get; set; }
        private IFabDailyTasks FabDailyTasks { get; set; }

        protected override void Init()
        {
            Profile = Get<CBSProfile>();
            FabDailyTasks = FabExecuter.Get<FabDailyTasks>();
        }

        // API methods

        /// <summary>
        /// Get list of all available tasks.
        /// </summary>
        /// <param name="result"></param>
        public void GetAllDailyTasks(Action<GetAllDailyTasksResult> result)
        {
            var profileID = Profile.PlayerID;

            FabDailyTasks.GetDailyTaskTable(profileID, onGet => {
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetAllDailyTasksResult { 
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawData = onGet.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var objectData = jsonPlugin.DeserializeObject<DailyTasksData>(rawData);

                    result?.Invoke(new GetAllDailyTasksResult {
                        IsSuccess = true,
                        TasksData = objectData
                    });
                }
            }, onFailed => {
                result?.Invoke(new GetAllDailyTasksResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Get daily tasks for current player.
        /// </summary>
        /// <param name="result"></param>
        public void GetPlayerDailyTasks(Action<GetPlayerDailyTasksResult> result)
        {
            var profileID = Profile.PlayerID;

            FabDailyTasks.GetDailyTasksState(profileID, onGet => {
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetPlayerDailyTasksResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawResult = onGet.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var tasksObject = jsonPlugin.DeserializeObject<PlayerTasksResponeData>(rawResult);

                    result?.Invoke(new GetPlayerDailyTasksResult
                    {
                        IsSuccess = true,
                        CurrentTasks = tasksObject.Tasks
                    });
                }
            }, onFailed => {
                result?.Invoke(new GetPlayerDailyTasksResult {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Adds a point to an task. For Tasks "OneShot" completes it immediately, for Tasks "Steps" - adds one step
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="result"></param>
        public void AddTaskPoint(string taskID, Action<ModifyTaskPointResult> result)
        {
            InternalModifyPoints(taskID, 1, ModifyMethod.ADD, result);
        }

        /// <summary>
        /// Adds the points you specified to the task. More suitable for Steps task.
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="points"></param>
        /// <param name="result"></param>
        public void AddTaskPoint(string taskID, int points, Action<ModifyTaskPointResult> result)
        {
            InternalModifyPoints(taskID, points, ModifyMethod.ADD, result);
        }

        /// <summary>
        /// Updates the task points you specified. More suitable for Steps tasks.
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="points"></param>
        /// <param name="result"></param>
        public void UpdateTaskPoint(string taskID, int points, Action<ModifyTaskPointResult> result)
        {
            InternalModifyPoints(taskID, points, ModifyMethod.UPDATE, result);
        }

        /// <summary>
        /// Pick up a reward from a completed task if it hasn't been picked up before.
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="result"></param>
        public void PickupTaskReward(string taskID, Action<ModifyTaskPointResult> result)
        {
            var profileID = Profile.PlayerID;

            FabDailyTasks.PickupReward(profileID, taskID, onPick => {
                if (onPick.Error != null)
                {
                    result?.Invoke(new ModifyTaskPointResult
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

                    result?.Invoke(new ModifyTaskPointResult
                    {
                        IsSuccess = true,
                        Task = resultObject.Task,
                        ReceivedReward = prize
                    });
                }
            }, onFailed => {
                result?.Invoke(new ModifyTaskPointResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Reset current player tasks state and get new random tasks.
        /// </summary>
        /// <param name="result"></param>
        public void ResetAndGetNewTasks(Action<GetPlayerDailyTasksResult> result)
        {
            var profileID = Profile.PlayerID;

            FabDailyTasks.ResetTasks(profileID, onReset => {
                if (onReset.Error != null)
                {
                    result?.Invoke(new GetPlayerDailyTasksResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onReset.Error)
                    });
                }
                else
                {
                    var rawResult = onReset.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var tasksObject = jsonPlugin.DeserializeObject<PlayerTasksResponeData>(rawResult);

                    var resetResult = new GetPlayerDailyTasksResult
                    {
                        IsSuccess = true,
                        CurrentTasks = tasksObject.Tasks
                    };

                    OnTaskReseted?.Invoke(resetResult);
                    result?.Invoke(resetResult);
                }
            }, onFailed => {
                result?.Invoke(new GetPlayerDailyTasksResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        // internal

        private void InternalModifyPoints(string taskID, int points, ModifyMethod modify, Action<ModifyTaskPointResult> result)
        {
            var profileID = Profile.PlayerID;

            FabDailyTasks.ModifyTasksPoint(profileID, taskID, points, modify, onAdd => {
                if (onAdd.Error != null)
                {
                    result?.Invoke(new ModifyTaskPointResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onAdd.Error)
                    });
                }
                else
                {
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
                        OnCompleteTask?.Invoke(new CompleteTaskResult
                        {
                            Task = resultObject.Task,
                            ReceivedReward = prize
                        });
                    }

                    result?.Invoke(new ModifyTaskPointResult
                    {
                        IsSuccess = true,
                        Task = resultObject.Task,
                        ReceivedReward = prize
                    });
                }
            }, onFailed => {
                Debug.Log(onFailed);
                result?.Invoke(new ModifyTaskPointResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }
    }

    public struct GetAllDailyTasksResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public DailyTasksData TasksData;
    }

    public struct GetPlayerDailyTasksResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public List<CBSTask> CurrentTasks;
    }

    public struct ModifyTaskPointResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public CBSTask Task;
        public PrizeObject ReceivedReward;
    }

    public struct CompleteTaskResult
    {
        public CBSTask Task;
        public PrizeObject ReceivedReward;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS
{
    public interface IDailyTasks
    {
        /// <summary>
        /// Notify when player complete task.
        /// </summary>
        event Action<CompleteTaskResult> OnCompleteTask;
        /// <summary>
        /// Notify when player receive reward for task.
        /// </summary>
        event Action<PrizeObject> OnPlayerRewarded;
        /// <summary>
        /// Notify when player reset tasks and get new
        /// </summary>
        event Action<GetPlayerDailyTasksResult> OnTaskReseted;

        /// <summary>
        /// Get list of all available tasks.
        /// </summary>
        /// <param name="result"></param>
        void GetAllDailyTasks(Action<GetAllDailyTasksResult> result);

        /// <summary>
        /// Get daily tasks for current player.
        /// </summary>
        /// <param name="result"></param>
        void GetPlayerDailyTasks(Action<GetPlayerDailyTasksResult> result);

        /// <summary>
        /// Adds a point to an task. For Tasks "OneShot" completes it immediately, for Tasks "Steps" - adds one step
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="result"></param>
        void AddTaskPoint(string taskID, Action<ModifyTaskPointResult> result);

        /// <summary>
        /// Adds the points you specified to the task. More suitable for Steps task.
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="points"></param>
        /// <param name="result"></param>
        void AddTaskPoint(string taskID, int points, Action<ModifyTaskPointResult> result);

        /// <summary>
        /// Updates the task points you specified. More suitable for Steps tasks.
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="points"></param>
        /// <param name="result"></param>
        void UpdateTaskPoint(string taskID, int points, Action<ModifyTaskPointResult> result);

        /// <summary>
        /// Pick up a reward from a completed task if it hasn't been picked up before.
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="result"></param>
        void PickupTaskReward(string taskID, Action<ModifyTaskPointResult> result);

        /// <summary>
        /// Reset current player tasks state and get new random tasks.
        /// </summary>
        /// <param name="result"></param>
        void ResetAndGetNewTasks(Action<GetPlayerDailyTasksResult> result);
    }
}

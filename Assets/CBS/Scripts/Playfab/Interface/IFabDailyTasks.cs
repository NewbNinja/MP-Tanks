using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public interface IFabDailyTasks
    {
        void GetDailyTaskTable(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void GetDailyTasksState(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void ModifyTasksPoint(string profileID, string taskID, int points, ModifyMethod method, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void PickupReward(string profileID, string taskID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void ResetTasks(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);
    }
}

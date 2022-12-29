using CBS.Utils;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public class FabDailyTasks : FabExecuter, IFabDailyTasks
    {
        public void GetDailyTaskTable(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetDailyTasksTableMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void GetDailyTasksState(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetDailyTasksMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    ZoneOfset = DateUtils.GetZoneOfset()
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void ModifyTasksPoint(string profileID, string taskID, int points, ModifyMethod method, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.AddDailyTaskPointsMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    Points = points,
                    Method = method,
                    TaskID = taskID
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void PickupReward(string profileID, string taskID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.PickupDailyTaskRewardMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    TaskID = taskID
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void ResetTasks(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.ResetDailyTasksMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    ZoneOfset = DateUtils.GetZoneOfset()
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }
    }
}
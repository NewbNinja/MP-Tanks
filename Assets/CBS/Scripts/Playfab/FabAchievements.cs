using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public class FabAchievements : FabExecuter, IFabAchievements
    {
        public void GetAchievementsTable(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetAchievementsTableMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void ModifyAchievementPoint(string profileID, string achievementID, int points, ModifyMethod method, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.AddAchievementPointsMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    Points = points,
                    Method = method,
                    AchievementID = achievementID
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void PickupReward(string profileID, string achievementID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.PickupAchievementRewardMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    AchievementID = achievementID
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void ResetAchievement(string profileID, string achievementID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.ResetAchievementMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    AchievementID = achievementID
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }
    }
}

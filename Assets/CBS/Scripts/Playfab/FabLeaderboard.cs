using PlayFab;
using PlayFab.ClientModels;
using PlayFab.CloudScriptModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public class FabLeaderboard : FabExecuter, IFabLeaderboard
    {
        public void GetLeaderboard(GetLeaderboardRequest leaderboardRequest, Action<ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetLeaderboardMethod,
                FunctionParameter = new
                {
                    profileID = leaderboardRequest.ProfileID,
                    statisticName = leaderboardRequest.StatisticName,
                    maxCount = leaderboardRequest.MaxCount
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void GetLeaderboardAroundPlayer(GetLeaderboardRequest leaderboardRequest, Action<ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetLeaderboardAroundPlayerMethod,
                FunctionParameter = new
                {
                    profileID = leaderboardRequest.ProfileID,
                    statisticName = leaderboardRequest.StatisticName,
                    maxCount = leaderboardRequest.MaxCount
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void GetFriendsLeaderboard(GetLeaderboardRequest leaderboardRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetFirendsLeaderboardMethod,
                FunctionParameter = new
                {
                    profileID = leaderboardRequest.ProfileID,
                    statisticName = leaderboardRequest.StatisticName,
                    maxCount = leaderboardRequest.MaxCount
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void GetLeaderboardOfClanAdmins(GetClanLeaderboardRequest leaderboardRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetClanAdminLeadersMethod,
                FunctionParameter = new
                {
                    viewerEntityID = leaderboardRequest.ViewerEntityID,
                    statisticName = leaderboardRequest.StatisticName,
                    maxCount = leaderboardRequest.MaxCount
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void UpdateStatisticPoint(UpdateStatisticRequest updateRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.UpdateStatisticMethod,
                FunctionParameter = new
                {
                    profileID = updateRequest.ProfileID,
                    statisticName = updateRequest.StatisticName,
                    value = updateRequest.Value
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void AddStatisticPoint(UpdateStatisticRequest updateRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.AddStatisticMethod,
                FunctionParameter = new
                {
                    profileID = updateRequest.ProfileID,
                    statisticName = updateRequest.StatisticName,
                    value = updateRequest.Value
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void AddClanStatisticPoint(UpdateClanStatisticRequest updateRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.AddClanStatisticMethod,
                FunctionParameter = new
                {
                    clanID = updateRequest.ClanID,
                    statisticName = updateRequest.StatisticName,
                    value = updateRequest.Value
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void UpdateClanStatisticPoint(UpdateClanStatisticRequest updateRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.UpdateClanStatisticMethod,
                FunctionParameter = new
                {
                    clanID = updateRequest.ClanID,
                    statisticName = updateRequest.StatisticName,
                    value = updateRequest.Value
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void ResetPlayerLeaderboards(string profileID, Action<ExecuteFunctionResult> OnReset, Action<PlayFabError> OnFailed)
        {
            var request = new ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.ResetPlayerStatisticsMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnReset, OnFailed);
        }
    }

    public struct GetLeaderboardRequest
    {
        public string ProfileID;
        public string StatisticName;
        public int MaxCount;
    }

    public struct GetClanLeaderboardRequest
    {
        public string ViewerEntityID;
        public string StatisticName;
        public int MaxCount;
    }

    public struct UpdateStatisticRequest
    {
        public string ProfileID;
        public string StatisticName;
        public int Value;
    }

    public struct UpdateClanStatisticRequest
    {
        public string ClanID;
        public string StatisticName;
        public int Value;
    }
}

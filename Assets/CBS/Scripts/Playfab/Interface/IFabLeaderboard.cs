using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public interface IFabLeaderboard
    {
        void GetLeaderboard(GetLeaderboardRequest leaderboardRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void GetLeaderboardAroundPlayer(GetLeaderboardRequest leaderboardRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void GetFriendsLeaderboard(GetLeaderboardRequest leaderboardRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void UpdateStatisticPoint(UpdateStatisticRequest updateRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void AddStatisticPoint(UpdateStatisticRequest updateRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void AddClanStatisticPoint(UpdateClanStatisticRequest updateRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void UpdateClanStatisticPoint(UpdateClanStatisticRequest updateRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void GetLeaderboardOfClanAdmins(GetClanLeaderboardRequest leaderboardRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void ResetPlayerLeaderboards(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnReset, Action<PlayFabError> OnFailed);
    }
}

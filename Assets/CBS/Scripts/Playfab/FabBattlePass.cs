using PlayFab;
using PlayFab.CloudScriptModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public class FabBattlePass : FabExecuter, IFabBattlePass
    {
        public void GetPlayerBattlePassStates(string profileID, string battlePassID, bool includeNotStarted, bool includeOutdated, Action<ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetPlayerBattlePassStatesMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    BattlePassID = battlePassID,
                    IncludeNotStarted = includeNotStarted,
                    IncludeOutdated = includeOutdated
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void GetBattlePassFullInformation(string profileID, string battlePassID, Action<ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetBattlePassFullInformationMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    BattlePassID = battlePassID
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void AddExpirienceToInstance(string profileID, int exp, string battlePassID, Action<ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.AddBattlePassExpirienceMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    BattlePassID = battlePassID,
                    Exp = exp
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void GetRewardFromInstance(string profileID, string battlePassID, int level, bool IsPremiun, Action<ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetRewardFromBattlePassInstanceMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    BattlePassID = battlePassID,
                    Level = level,
                    IsPremium = IsPremiun
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void GetPremiumAccess(string profileID, string battlePassID, Action<ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GrantPremiumAccessToBattlePassMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    BattlePassID = battlePassID
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void ResetInstanceForPlayer(string profileID, string battlePassID, Action<ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.ResetPlayerStateForBattlePassMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    BattlePassID = battlePassID
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }
    }
}

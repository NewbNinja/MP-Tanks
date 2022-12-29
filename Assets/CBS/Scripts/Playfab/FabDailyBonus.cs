using CBS.Utils;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public class FabDailyBonus : FabExecuter, IFabDailyBonus
    {
        public void GetDailyBonusState(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetDailyBonusMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    ZoneOfset = DateUtils.GetZoneOfset()
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void CollectDailyBonus(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.CollectDailyBonusMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    ZoneOfset = DateUtils.GetZoneOfset()
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void ResetDailyBonus(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnReset, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.ResetDailyBonusMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnReset, OnFailed);
        }
    }
}

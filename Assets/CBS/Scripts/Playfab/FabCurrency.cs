using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

namespace CBS.Playfab
{
    public class FabCurrency : FabExecuter, IFabCurrency
    {
        public void AddPlayerCurrerncy(string currency, int amount, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.AddCurrencyMethod,
                FunctionParameter = new { Currency = currency, Amount = amount }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnUpdate, OnFailed);
        }

        public void DecreasePlayerCurrerncy(string currency, int amount, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.DecreaseCurrencyMethod,
                FunctionParameter = new { Currency = currency, Amount = amount }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnUpdate, OnFailed);
        }

        public void GetUserCurrencies(Action<GetUserInventoryResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new GetUserInventoryRequest();
            PlayFabClientAPI.GetUserInventory(request, OnGet, OnFailed);
        }
    }
}

using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public interface IFabCurrency
    {
        void AddPlayerCurrerncy(string currency, int amount, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed);

        void DecreasePlayerCurrerncy(string currency, int amount, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed);

        void GetUserCurrencies(Action<GetUserInventoryResult> OnGet, Action<PlayFabError> OnFailed);
    }
}

using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFabDailyBonus
{
    void GetDailyBonusState(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

    void CollectDailyBonus(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

    void ResetDailyBonus(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnReset, Action<PlayFabError> OnFailed);
}

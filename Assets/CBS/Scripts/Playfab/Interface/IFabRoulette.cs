using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public interface IFabRoulette
    {
        void GetRouletteTable(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void SpinRoulette(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);
    }
}

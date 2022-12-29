using PlayFab;
using PlayFab.CloudScriptModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public interface IFabBattlePass
    {
        void GetPlayerBattlePassStates(string profileID, string battlePassID, bool includeNotStarted, bool includeOutdated, Action<ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);
        void GetBattlePassFullInformation(string profileID, string battlePassID, Action<ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);
        void AddExpirienceToInstance(string profileID, int exp, string battlePassID, Action<ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);
        void GetRewardFromInstance(string profileID, string battlePassID, int level, bool IsPremiun, Action<ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);
        void GetPremiumAccess(string profileID, string battlePassID, Action<ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);
        void ResetInstanceForPlayer(string profileID, string battlePassID, Action<ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);
    }
}

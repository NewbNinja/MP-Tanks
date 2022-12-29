using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public interface IFabClan
    {
        void CreateClan(CreateClanRequest clanRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnCreate, Action<PlayFabError> OnFailed);

        void GetClanInfo(string clanID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void RemoveClan(string clanID, string clanName, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void AcceptClanInvite(string clanID, string playerEntity, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void GetClanApplications(string clanID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void AcceptGroupApplication(string clanID, string playerEntity, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnAccept, Action<PlayFabError> OnFailed);

        void GetClanMemberShips(string clanID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void GetUserClan(string entityID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);
    }
}

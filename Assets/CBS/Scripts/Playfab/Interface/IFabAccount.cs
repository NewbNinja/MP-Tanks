using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public interface IFabAccount
    {
        void GetUserAccountInfo(string playerID, Action<GetAccountInfoResult> OnGet, Action<PlayFabError> OnFailed);

        void UpdateUserDisplayName(string _name, Action<UpdateUserTitleDisplayNameResult> OnUpdate, Action<PlayFabError> OnFailed);

        void AddPlayerExpirience(int newExp, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed);

        void GetPlayerExpirience(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed);

        void GetProfile(string profileID, bool loadLevel, bool loadClan, bool loadEntity, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> onGet, Action<PlayFabError> onFailed);

        void SetAvatarUrl(string imageUrl, Action<EmptyResponse> onSet, Action<PlayFabError> onFailed);

        void SavePlayerData(string key, string value, Action<UpdateUserDataResult> onSet, Action<PlayFabError> onFailed);

        void GetPlayerData(string profileID, string[] keys, Action<GetUserDataResult> onGet, Action<PlayFabError> onFailed);
    }
}

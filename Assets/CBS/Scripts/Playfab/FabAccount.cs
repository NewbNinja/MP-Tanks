using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Linq;
using CBS.Utils;

namespace CBS.Playfab
{
    public class FabAccount : FabExecuter, IFabAccount
    {

        public void GetUserAccountInfo(string playerID, Action<GetAccountInfoResult> OnGet, Action<PlayFabError> OnFailed)
        {
            GetAccountInfoRequest request = new GetAccountInfoRequest { PlayFabId = playerID };
            PlayFabClientAPI.GetAccountInfo(request, OnGet, OnFailed);
        }

        public void UpdateUserDisplayName(string name, Action<UpdateUserTitleDisplayNameResult> OnUpdate, Action<PlayFabError> OnFailed)
        {
            var request = new UpdateUserTitleDisplayNameRequest { DisplayName = name };
            PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnUpdate, OnFailed);
        }

        public void AddPlayerExpirience(int newExp, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.AddExperienceMethod,
                FunctionParameter = new {
                    expValue = newExp,
                    experienceKey = CBSConstants.StaticsticExpKey,
                    levelGroupId = CBSConstants.LevelTitleKey
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnUpdate, OnFailed);
        }

        public void GetPlayerExpirience(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetExperienceMethod,
                FunctionParameter = new {
                    ProfileID = profileID
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnUpdate, OnFailed);
        }

        public void GetProfile(string profileID, bool loadLevel, bool loadClan, bool loadEntity, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> onGet, Action<PlayFabError> onFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetPlayerProfileMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    LoadLevel = loadLevel,
                    LoadClan = loadClan,
                    LoadEntity = loadEntity,
                    AuthContext = CBSProfile.AuthenticationContex.ToJson()
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, onGet, onFailed);
        }

        public void SetAvatarUrl(string imageUrl, Action<EmptyResponse> onSet, Action<PlayFabError> onFailed)
        {
            var request = new UpdateAvatarUrlRequest {
                ImageUrl = imageUrl
            };
            PlayFabClientAPI.UpdateAvatarUrl(request, onSet, onFailed);
        }

        public void SavePlayerData(string key, string value, Action<UpdateUserDataResult> onSet, Action<PlayFabError> onFailed)
        {
            var data = new Dictionary<string, string>();
            data.Add(key, value);

            var request = new UpdateUserDataRequest {
                Data = data,
                Permission = UserDataPermission.Public
            };
            PlayFabClientAPI.UpdateUserData(request, onSet, onFailed);
        }

        public void GetPlayerData(string profileID, string [] keys, Action<GetUserDataResult> onGet, Action<PlayFabError> onFailed)
        {
            var request = new GetUserDataRequest {
                Keys = keys.ToList(),
                PlayFabId = profileID
            };
            PlayFabClientAPI.GetUserData(request, onGet, onFailed);
        }
    }
}
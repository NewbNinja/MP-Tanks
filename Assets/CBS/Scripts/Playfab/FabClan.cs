using CBS.Utils;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public class FabClan : FabExecuter, IFabClan
    {
        public void CreateClan(CreateClanRequest clanRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnCreate, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.CreateClanMethod,
                FunctionParameter = new {
                    Name = clanRequest.ClanName,
                    Description = clanRequest.ClanDescription,
                    ImageURL = clanRequest.ClanImageURL,
                    EntityID = clanRequest.PlayerEntity.Id,
                    EntityType = clanRequest.PlayerEntity.Type,
                    AuthContext = CBSProfile.AuthenticationContex.ToJson()
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnCreate, OnFailed);
        }

        public void GetClanInfo(string clanID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetClanMethod,
                FunctionParameter = new
                {
                    ClanID = clanID,
                    AuthContext = CBSProfile.AuthenticationContex.ToJson()
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void RemoveClan(string clanID, string clanName, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.RemoveClanMethod,
                FunctionParameter = new
                {
                    ClanID = clanID,
                    ClanName = clanName,
                    AuthContext = CBSProfile.AuthenticationContex.ToJson()
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void AcceptClanInvite(string clanID, string playerEntity, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.AcceptClanInviteMethod,
                FunctionParameter = new
                {
                    ClanID = clanID,
                    EntityID = playerEntity,
                    AuthContext = CBSProfile.AuthenticationContex.ToJson()
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void GetClanApplications(string clanID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetClanAppicationsMethod,
                FunctionParameter = new
                {
                    ClanID = clanID,
                    AuthContext = CBSProfile.AuthenticationContex.ToJson()
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void AcceptGroupApplication(string playerEntity, string clanID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnAccept, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.AcceptGroupApplicationMethod,
                FunctionParameter = new
                {
                    ClanID = clanID,
                    EntityID = playerEntity,
                    AuthContext = CBSProfile.AuthenticationContex.ToJson()
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnAccept, OnFailed);
        }

        public void GetClanMemberShips(string clanID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetClanMembershipsMethod,
                FunctionParameter = new
                {
                    ClanID = clanID,
                    AuthContext = CBSProfile.AuthenticationContex.ToJson()
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void GetUserClan(string entityID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetUserClanMethod,
                FunctionParameter = new
                {
                    EntityID = entityID,
                    AuthContext = CBSProfile.AuthenticationContex.ToJson()
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }
    }
}

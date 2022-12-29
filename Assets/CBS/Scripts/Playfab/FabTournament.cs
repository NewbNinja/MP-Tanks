using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public class FabTournament : FabExecuter, IFabTournament
    {
        public void GetTournamentState(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetTournamentStateMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void FindAndJoinTournament(string profileID, string playerEntityID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnJoin, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.FindAndJoinTournamentMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    PlayerEntityID = playerEntityID
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnJoin, OnFailed);
        }

        public void LeaveTournament(string profileID, string playerEntityID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.LeaveTournamentMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    PlayerEntityID = playerEntityID
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void UpdateTournamentPoint(string profileID, int point, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.UpdatePlayerTournamentPointMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    Point = point
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnUpdate, OnFailed);
        }

        public void AddTournamentPoint(string profileID, int point, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.AddPlayerTournamentPointMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    Point = point
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnUpdate, OnFailed);
        }

        public void FinishTournament(string profileID, string playerEntityID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnFinish, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.FinishTournamentMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    PlayerEntityID = playerEntityID
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnFinish, OnFailed);
        }

        public void GetTournamentDataByID(string tournamentID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetTournamentMethod,
                FunctionParameter = new
                {
                    TournamentID = tournamentID,
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void GetAllTournaments(Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetAllTournamentMethod,
                FunctionParameter = new{}
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }
    }
}

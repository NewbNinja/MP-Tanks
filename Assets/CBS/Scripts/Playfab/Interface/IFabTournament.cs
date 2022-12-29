using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFabTournament
{
    void GetTournamentState(string profileID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed);
    void FindAndJoinTournament(string profileID, string playerEntityID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnJoin, Action<PlayFabError> OnFailed);
    void LeaveTournament(string profileID, string playerEntityID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);
    void UpdateTournamentPoint(string profileID, int point, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed);
    void AddTournamentPoint(string profileID, int point, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed);
    void FinishTournament(string profileID, string playerEntityID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnFinish, Action<PlayFabError> OnFailed);
    void GetTournamentDataByID(string tournamentID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);
    void GetAllTournaments(Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);
}

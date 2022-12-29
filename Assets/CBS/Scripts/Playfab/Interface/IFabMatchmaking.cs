using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public interface IFabMatchmaking
    {
        void GetMatchmakingList(Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void CreateTicket(string queueName, int waitTime, string entityID, MatchmakingPlayerAttributes atributes, Action<CreateMatchmakingTicketResult> OnCreate, Action<PlayFabError> OnFailed);

        void GetMatchmakingTicket(string queueName, string ticketID, Action<GetMatchmakingTicketResult> OnGet, Action<PlayFabError> OnFailed);

        void GetQueue(string queueName, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed);

        void GetMatch(string queueName, string matchID, Action<GetMatchResult> OnGet, Action<PlayFabError> OnFailed);

        void CancelMatctForPlayer(string queueName, string entityID, Action<CancelAllMatchmakingTicketsForPlayerResult> OnCancel, Action<PlayFabError> OnFailed);
#if UNITY_EDITOR
        void UpdateMatchmakingQueue(MatchmakingQueueConfig queue, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed);

        void RemoveMatchmakingQueue(string queueName, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnRemove, Action<PlayFabError> OnFailed);
#endif
    }
}

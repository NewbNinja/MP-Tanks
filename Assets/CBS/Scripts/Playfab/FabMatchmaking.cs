using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public class FabMatchmaking : FabExecuter, IFabMatchmaking
    {
        public void GetMatchmakingList(Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetMatchmakingListMethod
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void CreateTicket(string queueName, int waitTime, string entityID, MatchmakingPlayerAttributes attributes, Action<CreateMatchmakingTicketResult> OnCreate, Action<PlayFabError> OnFailed)
        {
            var request = new CreateMatchmakingTicketRequest
            {
                Creator = new MatchmakingPlayer
                {
                    Attributes = attributes,
                    Entity = new PlayFab.MultiplayerModels.EntityKey
                    {
                        Id = entityID,
                        Type = CBSConstants.EntityPlayerType
                    }
                },
                QueueName = queueName,
                GiveUpAfterSeconds = waitTime
            };
            PlayFabMultiplayerAPI.CreateMatchmakingTicket(request, OnCreate, OnFailed);
        }

        public void GetMatchmakingTicket(string queueName, string ticketID, Action<GetMatchmakingTicketResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new GetMatchmakingTicketRequest
            {
                QueueName = queueName,
                TicketId = ticketID
            };
            PlayFabMultiplayerAPI.GetMatchmakingTicket(request, OnGet, OnFailed);
        }

        public void GetQueue(string queueName, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetMatchmakingQueueMethod,
                FunctionParameter = new
                {
                    QueueName = queueName
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnGet, OnFailed);
        }

        public void GetMatch(string queueName, string matchID, Action<GetMatchResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new GetMatchRequest
            {
                QueueName = queueName,
                MatchId = matchID,
                ReturnMemberAttributes = true
            };
            PlayFabMultiplayerAPI.GetMatch(request, OnGet, OnFailed);
        }

        public void CancelMatctForPlayer(string queueName, string entityID, Action<CancelAllMatchmakingTicketsForPlayerResult> OnCancel, Action<PlayFabError> OnFailed)
        {
            var request = new CancelAllMatchmakingTicketsForPlayerRequest
            {
                QueueName = queueName,
                Entity = new PlayFab.MultiplayerModels.EntityKey {
                    Id = entityID,
                    Type = CBSConstants.EntityPlayerType
                }
            };
            PlayFabMultiplayerAPI.CancelAllMatchmakingTicketsForPlayer(request, OnCancel, OnFailed);
        }

#if UNITY_EDITOR
        public void UpdateMatchmakingQueue(MatchmakingQueueConfig queue, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.UpdateMatchmakingQueueMethod,
                FunctionParameter = new {
                    Queue = queue.ToJson()
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnUpdate, OnFailed);
        }

        public void RemoveMatchmakingQueue(string queueName, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnRemove, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.RemoveMatchmakingQueueMethod,
                FunctionParameter = new
                {
                    QueueName = queueName
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnRemove, OnFailed);
        }
#endif
    }
}

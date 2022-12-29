using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public class FabChat : FabExecuter, IFabChat
    {
        public void UpdateMessageList(UpdateMessageListRequest updateRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.MessageListUpdateMethod,
                FunctionParameter = new
                {
                    senderID = updateRequest.SenderID,
                    reciverID = updateRequest.ReciverID,
                    rowKey = updateRequest.RowKey,
                    partitionKey = updateRequest.PartitionKey,
                    lastMessage = updateRequest.LastMessage
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnUpdate, OnFailed);
        }

        public void ClearUnreadMessage(string profileID, string userID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnClear, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.ClearUnreadMethod,
                FunctionParameter = new
                {
                    ProfileID = profileID,
                    UserID = userID
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnClear, OnFailed);
        }
    }

    public struct UpdateMessageListRequest
    {
        public string SenderID;
        public string ReciverID;
        public string PartitionKey;
        public string RowKey;
        public string LastMessage;
    }
}

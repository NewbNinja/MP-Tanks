using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public interface IFabChat
    {
        void UpdateMessageList(UpdateMessageListRequest updateRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed);
        void ClearUnreadMessage(string profileID, string userID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnClear, Action<PlayFabError> OnFailed);
    }
}

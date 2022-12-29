using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public interface IFabFriends
    {
        void GetFriendsList(Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> onGet, Action<PlayFabError> OnFailed);

        void SendFriendsRequest(string friendUserId, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnAdd, Action<PlayFabError> OnFailed);

        void RemoveFriend(string friendUserId, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> onRemove, Action<PlayFabError> OnFailed);

        void AcceptFriend(string friendUserId, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> onRemove, Action<PlayFabError> OnFailed);

        void ForceAddFriend(string friendUserId, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> onAccept, Action<PlayFabError> OnFailed);
    }
}

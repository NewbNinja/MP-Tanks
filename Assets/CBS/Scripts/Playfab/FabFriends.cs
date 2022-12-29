using PlayFab;
using PlayFab.ClientModels;
using System;

namespace CBS.Playfab
{
    public class FabFriends : FabExecuter, IFabFriends
    {
        public void GetFriendsList(Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> onGet, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GetFriendsListMethod,
                FunctionParameter = new{}
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, onGet, OnFailed);
        }

        public void SendFriendsRequest(string friendUserId, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnAdd, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.SendFriendRequestMethod,
                FunctionParameter = new { 
                    friendID = friendUserId
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnAdd, OnFailed);
        }

        public void RemoveFriend(string friendUserId, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> onRemove, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.RemoveFriendMethod,
                FunctionParameter = new
                {
                    friendID = friendUserId
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, onRemove, OnFailed);
        }

        public void AcceptFriend(string friendUserId, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> onAccept, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.AcceptFriendMethod,
                FunctionParameter = new
                {
                    friendID = friendUserId
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, onAccept, OnFailed);
        }

        public void ForceAddFriend(string friendUserId, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> onAccept, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.ForceAddFriendMethod,
                FunctionParameter = new
                {
                    friendID = friendUserId
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, onAccept, OnFailed);
        }
    }
}

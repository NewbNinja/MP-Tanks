using CBS.Playfab;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.ClientModels;
using System.Linq;
using PlayFab;

namespace CBS
{
    public class CBSFriends : CBSModule, IFriends
    {
        /// <summary>
        /// Notifies when a friend request has been approved.
        /// </summary>
        public event Action<AcceptFriendResult> OnFriendAccepted;
        /// <summary>
        /// Notifies when a friend request has been rejected.
        /// </summary>
        public event Action<RemoveFriendResult> OnFriendDeclined;
        /// <summary>
        /// Notifies when a new user has been added to your friends list.
        /// </summary>
        public event Action<AddFriendResult> OnFriendAdded;

        private IFabFriends FabFriends { get; set; }

        protected override void Init()
        {
            FabFriends = FabExecuter.Get<FabFriends>();
        }

        /// <summary>
        /// Get a list of your friends.
        /// </summary>
        /// <param name="result"></param>
        public void GetFriends(Action<GetFriendsResult> result)
        {
            FabFriends.GetFriendsList(onGet => {
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetFriendsResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawData = onGet.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var friendResult = jsonPlugin.DeserializeObject<GetFriendsListResult>(rawData);
                    var fabList = friendResult.Friends;
                    fabList = fabList.Where(x => x.Tags != null && x.Tags.Contains(CBSConstants.FriendAcceptTag)).ToList();
                    var cbsList = fabList.Select(x => new CBSFriendInfo(x)).ToList();
                    result?.Invoke(new GetFriendsResult
                    {
                        IsSuccess = true,
                        Friends = cbsList
                    });
                }
            }, onError => {
                Debug.LogError(onError.ErrorMessage);
                result?.Invoke(new GetFriendsResult {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                });
            });
        }

        /// <summary>
        /// Get a list of users who want to be friends with you.
        /// </summary>
        /// <param name="result"></param>
        public void GetRequestedFriends(Action<GetFriendsResult> result)
        {
            FabFriends.GetFriendsList(onGet => {
                var rawData = onGet.FunctionResult.ToString();
                var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                var friendResult = jsonPlugin.DeserializeObject<GetFriendsListResult>(rawData);
                var fabList = friendResult.Friends;
                fabList = fabList.Where(x => x.Tags != null && x.Tags.Contains(CBSConstants.FriendRequestTag)).ToList();
                var cbsList = fabList.Select(x => new CBSFriendInfo(x)).ToList();
                result?.Invoke(new GetFriendsResult
                {
                    IsSuccess = true,
                    Friends = cbsList
                });
            }, onError => {
                result?.Invoke(new GetFriendsResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                });
            });
        }

        /// <summary>
        /// Checks if a user is on your friends list.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="result"></param>
        public void IsInFriends(string userID, Action<IsInFriendsResult> result)
        {
            FabFriends.GetFriendsList(onGet => {
                var rawData = onGet.FunctionResult.ToString();
                var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                var friendResult = jsonPlugin.DeserializeObject<GetFriendsListResult>(rawData);
                var fabList = friendResult.Friends;
                var requestList = fabList.Where(x => x.Tags != null && x.Tags.Contains(CBSConstants.FriendRequestTag)).ToList();
                var acceptList = fabList.Where(x => x.Tags != null && x.Tags.Contains(CBSConstants.FriendAcceptTag)).ToList();
                var waitList = fabList.Where(x => x.Tags == null).ToList();
                bool accepted = acceptList.Any(x => x.FriendPlayFabId == userID);
                bool requested = requestList.Any(x => x.FriendPlayFabId == userID);
                bool waited = waitList.Any(x => x.FriendPlayFabId == userID);

                result?.Invoke(new IsInFriendsResult
                {
                    IsSuccess = true,
                    FriendID = userID,
                    ExistAsAcceptedFriend = accepted,
                    ExistAsRequestedFriend = requested,
                    WaitForUserAccept = waited
                });
            }, onError => {
                result?.Invoke(new IsInFriendsResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                });
            });
        }

        /// <summary>
        /// Send user a friend request.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="result"></param>
        public void SendFriendsRequest(string userID, Action<SendFriendsRequestResult> result)
        {
            FabFriends.SendFriendsRequest(userID, onSend => {
                if (onSend.Error != null)
                {
                    result?.Invoke(new SendFriendsRequestResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onSend.Error)
                    });
                }
                else
                {
                    Debug.Log(onSend.FunctionResult);
                    result?.Invoke(new SendFriendsRequestResult {
                        IsSuccess = true,
                        FriendsID = userID
                    });
                }
            }, onError => {
                result?.Invoke(new SendFriendsRequestResult {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                });
            });
        }

        /// <summary>
        /// Remove user from friends.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="result"></param>
        public void RemoveFriend(string userID, Action<RemoveFriendResult> result)
        {
            FabFriends.RemoveFriend(userID, onSend => {
                Debug.Log(onSend.FunctionResult);
                if (onSend.Error != null)
                {
                    result?.Invoke(new RemoveFriendResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onSend.Error)
                    });
                }
                else
                {
                    var callback = new RemoveFriendResult
                    {
                        IsSuccess = true,
                        FriendsID = userID
                    };
                    result?.Invoke(callback);
                    OnFriendDeclined?.Invoke(callback);
                }
            }, onError => {
                result?.Invoke(new RemoveFriendResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                });
            });
        }

        /// <summary>
        /// Reject a user's friend request.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="result"></param>
        public void DeclineFriendRequest(string userID, Action<RemoveFriendResult> result)
        {
            RemoveFriend(userID, result);
        }

        /// <summary>
        /// Approve the user's friend request.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="result"></param>
        public void AcceptFriend(string userID, Action<AcceptFriendResult> result)
        {
            FabFriends.AcceptFriend(userID, onSend => {
                if (onSend.Error != null)
                {
                    result?.Invoke(new AcceptFriendResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onSend.Error)
                    });
                }
                else
                {
                    var callback = new AcceptFriendResult
                    {
                        IsSuccess = true,
                        FriendsID = userID
                    };
                    result?.Invoke(callback);
                    OnFriendAccepted?.Invoke(callback);
                    OnFriendAdded?.Invoke(new AddFriendResult {
                        IsSuccess = true,
                        FriendsID = userID
                    });
                }
            }, onError => {
                result?.Invoke(new AcceptFriendResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                });
            });
        }

        /// <summary>
        /// Add user to friends without confirmation.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="result"></param>
        public void ForceAddFriend(string userID, Action<AddFriendResult> result)
        {
            FabFriends.ForceAddFriend(userID, onSend => {
                if (onSend.Error != null)
                {
                    result?.Invoke(new AddFriendResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onSend.Error)
                    });
                }
                else
                {
                    var callback = new AddFriendResult
                    {
                        IsSuccess = true,
                        FriendsID = userID
                    };
                    result?.Invoke(callback);
                    OnFriendAdded?.Invoke(callback);
                }
            }, onError => {
                result?.Invoke(new AddFriendResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                });
            });
        }

    }

    public class CBSFriendInfo
    {
        public string ProfileID { get; private set; }
        public string DisplayName { get; private set; }
        public string AvatarUrl { get; private set; }

        public CBSFriendInfo(FriendInfo friend)
        {
            ProfileID = friend.FriendPlayFabId;
            DisplayName = friend.Profile == null ? string.Empty : friend.Profile.DisplayName;
            AvatarUrl = friend.Profile == null ? string.Empty : friend.Profile.AvatarUrl;
        }
    }

    public struct GetFriendsResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public List<CBSFriendInfo> Friends;
    }

    public struct IsInFriendsResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string FriendID;
        public bool ExistAsAcceptedFriend;
        public bool ExistAsRequestedFriend;
        public bool WaitForUserAccept;
    }

    public struct SendFriendsRequestResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string FriendsID;
    }

    public struct RemoveFriendResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string FriendsID;
    }

    public struct AcceptFriendResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string FriendsID;
    }

    public struct AddFriendResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string FriendsID;
    }
}

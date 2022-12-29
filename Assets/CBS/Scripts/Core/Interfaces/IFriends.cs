using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS
{
    public interface IFriends
    {
        /// <summary>
        /// Notifies when a friend request has been approved.
        /// </summary>
        event Action<AcceptFriendResult> OnFriendAccepted;
        /// <summary>
        /// Notifies when a friend request has been rejected.
        /// </summary>
        event Action<RemoveFriendResult> OnFriendDeclined;
        /// <summary>
        /// Notifies when a new user has been added to your friends list.
        /// </summary>
        event Action<AddFriendResult> OnFriendAdded;

        /// <summary>
        /// Get a list of your friends.
        /// </summary>
        /// <param name="result"></param>
        void GetFriends(Action<GetFriendsResult> result);

        /// <summary>
        /// Get a list of users who want to be friends with you.
        /// </summary>
        /// <param name="result"></param>
        void GetRequestedFriends(Action<GetFriendsResult> result);
        /// <summary>
        /// Checks if a user is on your friends list.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="result"></param>
        void IsInFriends(string userID, Action<IsInFriendsResult> result);
        /// <summary>
        /// Send user a friend request.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="result"></param>
        void SendFriendsRequest(string userID, Action<SendFriendsRequestResult> result);
        /// <summary>
        /// Remove user from friends.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="result"></param>
        void RemoveFriend(string userID, Action<RemoveFriendResult> result);
        /// <summary>
        /// Reject a user's friend request.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="result"></param>
        void DeclineFriendRequest(string userID, Action<RemoveFriendResult> result);
        /// <summary>
        /// Approve the user's friend request.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="result"></param>
        void AcceptFriend(string userID, Action<AcceptFriendResult> result);
        /// <summary>
        /// Add user to friends without confirmation.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="result"></param>
        void ForceAddFriend(string userID, Action<AddFriendResult> result);
    }
}

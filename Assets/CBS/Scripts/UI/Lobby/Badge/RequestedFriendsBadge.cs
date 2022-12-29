using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class RequestedFriendsBadge : BaseBadge
    {
        private IFriends Friends { get; set; }

        private void Awake()
        {
            Friends = CBSModule.Get<CBSFriends>();
            Back.SetActive(false);
        }

        protected virtual void OnEnable()
        {
            Friends.GetRequestedFriends(OnFriendsGetted);
            // add listeners
            Friends.OnFriendAccepted += OnFriendAccepted;
            Friends.OnFriendDeclined += OnFriendDeclined;
            StartUpdateInterval();
        }

        protected virtual void OnDisable()
        {
            // remove listeners
            Friends.OnFriendAccepted -= OnFriendAccepted;
            Friends.OnFriendDeclined -= OnFriendDeclined;
            StopUpdateInterval();
        }

        private void OnFriendsGetted(GetFriendsResult result)
        {
            var requestedFriends = result.Friends;
            UpdateCount(requestedFriends.Count);
        }

        // events
        private void OnFriendAccepted(AcceptFriendResult obj)
        {
            Friends.GetRequestedFriends(OnFriendsGetted);
        }


        private void OnFriendDeclined(RemoveFriendResult obj)
        {
            Friends.GetRequestedFriends(OnFriendsGetted);
        }

        protected override void OnUpdateInterval()
        {
            Debug.Log("OnUpdateInterval");
            Friends.GetRequestedFriends(OnFriendsGetted);
        }
    }
}

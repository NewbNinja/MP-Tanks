using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS.UI
{
    public class FriendsBadge : BaseBadge
    {
        private IFriends Friends { get; set; }
        private ICBSChat CBSChat { get; set; }

        private int FriendsCount { get; set; }
        private int MessageCount { get; set; }

        private void Awake()
        {
            CBSChat = CBSModule.Get<CBSChat>();
            Friends = CBSModule.Get<CBSFriends>();
            Back.SetActive(false);
        }

        protected virtual void OnEnable()
        {
            Friends.GetRequestedFriends(OnFriendsGetted);
            CBSChat.GetUserDialogList(OnUserDialogGet);

            // add listeners
            CBSChat.OnUnreadMessageClear += OnMessageClear;
            Friends.OnFriendAccepted += OnFriendAccepted;
            Friends.OnFriendDeclined += OnFriendDeclined;
            StartUpdateInterval();
        }

        protected virtual void OnDisable()
        {
            // remove listeners
            CBSChat.OnUnreadMessageClear -= OnMessageClear;
            Friends.OnFriendAccepted -= OnFriendAccepted;
            Friends.OnFriendDeclined -= OnFriendDeclined;
            StopUpdateInterval();
        }

        private void OnFriendsGetted(GetFriendsResult result)
        {
            var requestedFriends = result.Friends;
            if (requestedFriends != null)
            {
                FriendsCount = requestedFriends.Count;
                UpdateCount(FriendsCount + MessageCount);
            }
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
            Friends.GetRequestedFriends(OnFriendsGetted);
        }

        private void OnUserDialogGet(GetDialogListResult result)
        {
            var list = result.Dialogs;
            if (list == null)
                return;
            MessageCount = list.Select(x => int.Parse(x.UnreadCount)).Sum();
            UpdateCount(MessageCount + FriendsCount);
        }

        private void OnMessageClear(string userID)
        {
            CBSChat.GetUserDialogList(OnUserDialogGet);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class RequestedView : FriendsSection
    {
        [SerializeField]
        private FriendsListScroller Scroller;

        public override FriendsTabTitle TabTitle => FriendsTabTitle.REQUESTED;

        private void Start()
        {
            Friends.OnFriendDeclined += OnFriendDeclined;
            Friends.OnFriendAccepted += OnFriendAccepted;
        }

        private void OnDestroy()
        {
            Friends.OnFriendDeclined -= OnFriendDeclined;
            Friends.OnFriendAccepted -= OnFriendAccepted;
        }

        public override void Clean()
        {
            Scroller.HideAll();
        }

        public override void Display()
        {
            Friends.GetRequestedFriends(OnFriendsGet);
        }

        // events
        private void OnFriendsGet(GetFriendsResult result)
        {
            var uiPrefab = Prefabs.RequestedFriendUI;
            var list = result.Friends;
            Scroller.Spawn(uiPrefab, list);
        }

        private void OnFriendDeclined(RemoveFriendResult obj)
        {
            Display();
        }

        private void OnFriendAccepted(AcceptFriendResult obj)
        {
            Display();
        }
    }
}

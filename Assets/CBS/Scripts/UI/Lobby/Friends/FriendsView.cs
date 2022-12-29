using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class FriendsView : FriendsSection
    {
        [SerializeField]
        private FriendsListScroller Scroller;

        public override FriendsTabTitle TabTitle => FriendsTabTitle.FRIENDS;

        private void Start()
        {
            Friends.OnFriendDeclined += OnFriendRemoved;
        }

        private void OnDestroy()
        {
            Friends.OnFriendDeclined -= OnFriendRemoved;
        }

        public override void Clean()
        {
            Scroller.HideAll();
        }

        public override void Display()
        {
            Friends.GetFriends(OnFriendsGet);
        }

        // events

        private void OnFriendsGet(GetFriendsResult result)
        {
            var uiPrefab = Prefabs.FriendUI;
            var list = result.Friends;
            Scroller.Spawn(uiPrefab, list);
        }

        private void OnFriendRemoved(RemoveFriendResult obj)
        {
            Display();
        }
    }
}
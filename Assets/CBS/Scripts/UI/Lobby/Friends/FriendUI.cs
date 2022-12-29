using CBS.Core;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class FriendUI : MonoBehaviour, IScrollableItem<CBSFriendInfo>
    {
        [SerializeField]
        private Text DisplayName;
        [SerializeField]
        private AvatarDrawer Avatar;

        protected CBSFriendInfo CurrentFriend { get; set; }

        protected IFriends Friends { get; set; }

        private void Awake()
        {
            Friends = CBSModule.Get<CBSFriends>();
        }

        public void Display(CBSFriendInfo data)
        {
            CurrentFriend = data;
            DisplayName.text = CurrentFriend.DisplayName;

            var url = data.AvatarUrl;
            var id = data.ProfileID;
            if (gameObject.activeInHierarchy)
            {
                Avatar.LoadAvatarFromUrl(url, id);
                Avatar.SetClickable(data.ProfileID);
            }
        }

        public void ShowInfo()
        {
            string userID = CurrentFriend.ProfileID;
            new PopupViewer().ShowUserInfo(userID);
        }
    }
}

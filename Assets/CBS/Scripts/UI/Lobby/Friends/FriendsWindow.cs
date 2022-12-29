using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace CBS.UI
{
    public class FriendsWindow : MonoBehaviour
    {
        [SerializeField]
        private BaseFriendsTab [] AllTabes;
        [SerializeField]
        private FriendsSection[] AllView;
        [SerializeField]
        private GameObject PrivateChat;

        private FriendsTabTitle CurrentTab { get; set; }

        private void Awake()
        {
            foreach (var tab in AllTabes)
                tab.SetSelectAction(OnTabSelected);
        }

        private void OnEnable()
        {
            DisaplyView(CurrentTab);
        }

        private void OnTabSelected(string title)
        {
            CurrentTab = (FriendsTabTitle)Enum.Parse(typeof(FriendsTabTitle), title, true);
            DisaplyView(CurrentTab);
        }

        private void DisaplyView(FriendsTabTitle title)
        {
            PrivateChat.SetActive(false);
            foreach (var view in AllView)
            {
                view.Clean();
                view.Hide();
            }
            var activeView = AllView.FirstOrDefault(x => x.TabTitle == title);
            if (activeView != null)
            {
                activeView.SetChatAction(OnChatSelected);
                activeView.gameObject.SetActive(true);
                activeView.Display();
            }
        }

        private void OnChatSelected(string userID)
        {
            foreach (var view in AllView)
            {
                view.Clean();
                view.Hide();
            }
            PrivateChat.SetActive(true);
            PrivateChat.GetComponent<PrivateMessageView>().Setup(userID);
        }
    }

    public enum FriendsTabTitle
    {
        FRIENDS,
        REQUESTED,
        MESSAGES
    }
}

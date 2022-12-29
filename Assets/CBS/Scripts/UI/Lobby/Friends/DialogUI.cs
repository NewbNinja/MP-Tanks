using CBS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class DialogUI : MonoBehaviour, IScrollableItem<MessageDialogObject>
    {
        [SerializeField]
        private Text DisplayName;
        [SerializeField]
        private Text Message;
        [SerializeField]
        private Text BadgeCount;
        [SerializeField]
        private GameObject BadgeBody;
        [SerializeField]
        private AvatarDrawer Avatar;

        private MessageDialogObject Dialog { get; set; }
        private Action<string> ClickAction { get; set; }

        private ICBSChat CBSChat { get; set; }

        private void Awake()
        {
            CBSChat = CBSModule.Get<CBSChat>();
            CBSChat.OnUnreadMessageClear += OnUnreadMessageClear;
        }

        private void OnDestroy()
        {
            CBSChat.OnUnreadMessageClear -= OnUnreadMessageClear;
        }

        public void Display(MessageDialogObject data)
        {
            Dialog = data;
            // draw ui
            DisplayName.text = Dialog.UserName;
            Message.text = Dialog.LastMessage;
            Avatar.LoadProfileAvatar(Dialog.UserID);
            Avatar.SetClickable(Dialog.UserID);

            DrawBadge();
        }

        private void DrawBadge()
        {
            int unreadCount = int.Parse(Dialog.UnreadCount);
            BadgeBody.SetActive(unreadCount > 0);
            BadgeCount.text = Dialog.UnreadCount;
        }

        public void OnClickDialog()
        {
            ClickAction?.Invoke(Dialog.UserID);
        }

        public void SetClickAction(Action<string> action)
        {
            ClickAction = action;
        }

        // events
        private void OnUnreadMessageClear(string userID)
        {
            if (Dialog == null)
                return;
            if (Dialog.UserID == userID)
            {
                Dialog.UnreadCount = "0";
                DrawBadge();
            }
        }
    }
}

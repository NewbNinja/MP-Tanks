using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CBS.UI
{
    public class UnreadMessageBadge : BaseBadge
    {
        private ICBSChat CBSChat { get; set; }

        private void Awake()
        {
            CBSChat = CBSModule.Get<CBSChat>();
            Back.SetActive(false);
        }

        private void OnEnable()
        {
            CBSChat.GetUserDialogList(OnUserDialogGet);

            CBSChat.OnUnreadMessageClear += OnMessageClear;
        }

        private void OnDisable()
        {
            CBSChat.OnUnreadMessageClear -= OnMessageClear;
        }

        // events
        private void OnUserDialogGet(GetDialogListResult result)
        {
            var list = result.Dialogs;
            if (list == null)
                return;
            int unreadCount = list.Select(x => int.Parse(x.UnreadCount)).Sum();
            UpdateCount(unreadCount);
        }

        private void OnMessageClear(string userID)
        {
            CBSChat.GetUserDialogList(OnUserDialogGet);
        }
    }
}

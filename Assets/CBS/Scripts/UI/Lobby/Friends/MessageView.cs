using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class MessageView : FriendsSection
    {
        [SerializeField]
        private MessageDialogScroller Scroller;

        public override FriendsTabTitle TabTitle => FriendsTabTitle.MESSAGES;

        public override void Clean()
        {
            Scroller.HideAll();
        }

        public override void Display()
        {
            Chats.GetUserDialogList(onGet => {
                if (onGet.IsSuccess)
                {
                    var dialogList = onGet.Dialogs;
                    var dialogPrefab = Prefabs.DialogUI;

                    var dialogPool = Scroller.Spawn(dialogPrefab, dialogList);
                    foreach (var dialog in dialogPool)
                        dialog.GetComponent<DialogUI>().SetClickAction(SelectChatAction);
                }
            });
        }
    }
}

using CBS.Core;
using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class SimpleChatMessage : MonoBehaviour, IScrollableItem<MessageBody>
    {
        [SerializeField]
        private Text Body;

        private MessageBody Message { get; set; }

        private RectTransform RectTransform { get; set; }

        private Vector2 DefaultSize { get; set; }

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            DefaultSize = RectTransform.sizeDelta;
        }

        public void Display(MessageBody message)
        {
            Message = message;
            // bold nickname
            string nickname = ChatUtils.ConvertNickname(message.SenderName);
            string body = Message.Body;
            string full = nickname + " " + body;
            // check is mine
            if (Message.IsMine)
            {
                full = ChatUtils.MarkAsMine(full);
            }
            // finaly apply text
            Body.text = full;

            ResizeBody();
        }

        private void ResizeBody()
        {
            float preferredHeight = Body.preferredHeight;
            float defaultHeight = DefaultSize.y;
            float height = preferredHeight > defaultHeight ? preferredHeight : defaultHeight;
            RectTransform.sizeDelta = new Vector2(DefaultSize.x, height);
        }

        // button events
        public void ClickNickName()
        {
            string userID = Message.SenderID;
            new PopupViewer().ShowUserInfo(userID);
        }
    }
}

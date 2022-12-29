using CBS.Core;
using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class BubbleChatMessage : MonoBehaviour, IScrollableItem<MessageBody>
    {
        [SerializeField]
        private Text Body;
        [SerializeField]
        private Text Nickname;
        [SerializeField]
        private Text Date;
        [SerializeField]
        private Image Bubble;
        [SerializeField]
        private AvatarDrawer Avatar;

        private MessageBody Message { get; set; }

        private RectTransform TextRect { get; set; }
        private RectTransform BubbleRect { get; set; }
        private RectTransform RootRect { get; set; }

        private Vector2 DefaultTextSize { get; set; }
        private Vector2 DefaultBubbleSize { get; set; }
        private Vector2 DefaultRootSize { get; set; }

        private void Awake()
        {
            TextRect = Body.GetComponent<RectTransform>();
            BubbleRect = Bubble.GetComponent<RectTransform>();
            RootRect = GetComponent<RectTransform>();
            DefaultTextSize = TextRect.sizeDelta;
            DefaultBubbleSize = BubbleRect.sizeDelta;
            DefaultRootSize = RootRect.sizeDelta;
        }

        public void Display(MessageBody message)
        {
            //reset size
            TextRect.sizeDelta = DefaultTextSize;
            BubbleRect.sizeDelta = DefaultBubbleSize;
            RootRect.sizeDelta = DefaultRootSize;

            Message = message;
            // bold nickname
            string nickname = message.SenderName;
            string body = Message.Body;
            // check is mine
            if (Message.IsMine)
            {
                var newColor = ChatUtils.GetMineBubbleColor();
                Bubble.color = newColor;
            }

            Nickname.text = nickname;
            Body.text = body;

            // draw date
            var date = DateUtils.GetDateFromString(Message.TimeStamp);
            Date.text = date.ToString("MM/dd/yyyy H:mm");

            // draw avatars
            var profileID = message.SenderID;
            Avatar.LoadProfileAvatar(profileID);

            ResizeBody();
        }

        private void ResizeBody()
        {
            float preferredHeight = Body.preferredHeight;
            float defaultHeight = DefaultTextSize.y;
            float height = preferredHeight > defaultHeight ? preferredHeight : defaultHeight;
            TextRect.sizeDelta = new Vector2(DefaultTextSize.x, height);

            float bubbleHeight = BubbleRect.sizeDelta.y;
            float upHeight = preferredHeight > defaultHeight ? preferredHeight - defaultHeight : 0;
            bubbleHeight += upHeight;
            BubbleRect.sizeDelta = new Vector2(DefaultBubbleSize.x, bubbleHeight);

            RootRect.sizeDelta = new Vector2(RootRect.sizeDelta.x, RootRect.sizeDelta.y + upHeight);
        }
    }
}

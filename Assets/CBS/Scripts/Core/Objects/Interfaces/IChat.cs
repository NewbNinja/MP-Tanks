using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS
{
    public interface IChat
    {
        string ChatID { get; }

        int LoadMessagedAtStart { get; }

        void AddOnNewMessageListener(Action<MessageBody> newMessageAction);

        void RemoveOnNewMessageListener(Action<MessageBody> newMessageAction);

        void SendMessage(MessageBody message, Action<SendMessageResult> result = null);

        void GetMessages(Action<GetMessagesResult> result);
    }
}

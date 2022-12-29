using CBS.Core;
using CBS.Scriptable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class ChatView : MonoBehaviour, IDisposable
    {
        [SerializeField]
        private InputField MessageInput;
        [SerializeField]
        private ChatScroller Scroller;

        private IChat Chat { get; set; }
        private bool IsInited { get; set; }

        private IProfile Profile { get; set; }
        private ChatPrefabs Prefabs { get; set; }
        private GameObject MessagePrefab { get; set; }

        private void Awake()
        {
            Prefabs = CBSScriptable.Get<ChatPrefabs>();
            Profile = CBSModule.Get<CBSProfile>();
            MessagePrefab = MessagePrefab ?? Prefabs.SimpleChatMessage;
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void OnDisable()
        {
            Dispose();
        }

        public void Init(IChat chat)
        {
            // remove last chat if exist
            Dispose();
            // init new chat
            IsInited = true;
            Chat = chat;
            Chat.AddOnNewMessageListener(OnNewMessageAdded);
            Chat.GetMessages(OnGetMessage);
        }

        public void SendMessage()
        {
            if (!IsInited)
                return;
            string messageBody = MessageInput.text;
            if (string.IsNullOrEmpty(messageBody))
                return;
            // clear message
            MessageInput.text = string.Empty;
            var message = GenerateMessage(messageBody);
            Chat.SendMessage(message, onSent => {
                
            });
        }

        // internal
        private MessageBody GenerateMessage(string message)
        {
            return new MessageBody {
                ID = System.Guid.NewGuid().ToString(),
                Body = message,
                SenderID = Profile.PlayerID,
                SenderName = Profile.DisplayName
            };
        }

        // events
        public void OnNewMessageAdded(MessageBody message)
        {
            Scroller.PushNewMessage(message);
        }

        public void SetMessagePrefab(GameObject prefab)
        {
            MessagePrefab = prefab;
        }

        public void OnGetMessage(GetMessagesResult result)
        {
            if (result.IsSuccess)
            {
                if (Chat == null)
                    return;
                Scroller.SetPoolCount(Chat.LoadMessagedAtStart);
                Scroller.Spawn(MessagePrefab, result.List);
                Scroller.SetScrollPosition(0);
            }
        }

        private async void TestMessageLoop()
        {
            int counter = 0;
            while(true)
            {
                await Task.Delay(1000);
                var messageText = "Message " + counter.ToString();
                var message = GenerateMessage(messageText);
                Chat.SendMessage(message, onSent => {

                });
                counter++;
            }
        }

        public void Dispose()
        {
            Scroller.HideAll();
            IsInited = false;
            Chat?.RemoveOnNewMessageListener(OnNewMessageAdded);
            (Chat as IDisposable)?.Dispose();
            Chat = null;
        }
    }
}

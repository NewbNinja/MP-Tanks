using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CBS.Playfab;
using System.Threading.Tasks;
using System.Linq;
using CBS.Scriptable;
using PlayFab;

namespace CBS
{
    public class ChatInstance : IChat, IDisposable
    {
        public string ChatID { get; private set; }

        public int LoadMessagedAtStart { get; set; }

        private int NewMessageCompareCount { get; set; }

        private int CompareWait { get; set; }

        private Action<MessageBody> NewMessageAction { get; set; }

        private List<MessageBody> CacheMessages { get; set; }

        private IFabAzure FabAzure { get; set; }

        private IFabChat FabChat { get; set; }

        private IProfile Profile { get; set; }

        private ChatConfig ChatConfig { get; set; }

        private bool CompareIsRunning { get; set; }

        private ChatType ChatType { get; set; }

        private string[] UsersIds { get; set; }

        public ChatInstance(ChatRequest request)
        {
            LoadMessagedAtStart = request.LoadAtStart;
            FabAzure = FabExecuter.Get<FabAzure>();
            FabChat = FabExecuter.Get<FabChat>();
            Profile = CBSModule.Get<CBSProfile>();
            ChatConfig = CBSScriptable.Get<ChatConfig>();
            ChatID = request.ChatID;
            ChatType = request.Type;
            UsersIds = request.UsersIds;
            NewMessageCompareCount = CBSConstants.ChatCompareCount;
            CompareWait = CBSConstants.ChatCompareWait;
        }

        public void AddOnNewMessageListener(Action<MessageBody> newMessageAction)
        {
            NewMessageAction += newMessageAction;
            if (!CompareIsRunning)
                StartNewMessageTask();
        }

        public void RemoveOnNewMessageListener(Action<MessageBody> newMessageAction)
        {
            NewMessageAction -= newMessageAction;
            if (NewMessageAction == null || NewMessageAction.GetInvocationList() == null || NewMessageAction.GetInvocationList().Length == 0)
            {
                StopNewMessageTask();
            }
        }

        public void SendMessage(MessageBody message, Action<SendMessageResult> result = null)
        {
            // check message
            string bodyText = message.Body;
            message.Body = FixMessageLength(bodyText);
            // check name
            string nickname = message.SenderName;
            message.SenderName = FixNickname(nickname);

            string tableId = CBSConstants.ChatTablePrefix + ChatID;
            string rawData = JsonUtility.ToJson(message);

            var messageRequest = new AzureInsertDataRequest {
                RowKey = message.ID,
                PartitionKey = string.Empty,
                TableId = tableId,
                CreateTableIfNotExist = true,
                RawData = rawData
            };
            FabAzure.InsertDataToTable(messageRequest, onSend => {
                if (onSend.Error != null)
                {
                    result?.Invoke(new SendMessageResult {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onSend.Error)
                    });
                }
                else
                {
                    result?.Invoke(new SendMessageResult
                    {
                        IsSuccess = true
                    });
                    // update message list
                    if (ChatType == ChatType.PRIVATE)
                    {
                        UpdateMessageList(message);
                    }
                }
            }, onError => {
                result?.Invoke(new SendMessageResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                });
            });
        }

        public void GetMessages(Action<GetMessagesResult> result)
        {
            string tableId = CBSConstants.ChatTablePrefix + ChatID;
            FabAzure.GetDataFromTable(new AzureGetDataRequest { 
                TableId = tableId,
                nTop = LoadMessagedAtStart
            }, onGet => {
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetMessagesResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var resultList = new List<MessageBody>();
                    var rawData = onGet.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var dataObject = jsonPlugin.DeserializeObject<TableResult>(rawData);
                    var messageList = dataObject.value;
                    
                    foreach (var messageRaw in messageList)
                    {
                        var message = jsonPlugin.DeserializeObject<MessageBody>(messageRaw.RawData);
                        message.TimeStamp = messageRaw.Timestamp;
                        resultList.Add(message);
                    }
                    var sortedMessageList = resultList.OrderBy(x => x.TimeStamp).ToList();
                    CacheMessages = resultList ?? new List<MessageBody>();
                    result?.Invoke(new GetMessagesResult
                    {
                        IsSuccess = true,
                        List = sortedMessageList
                    });
                }
            }, onError => {
                Debug.Log(onError.ErrorMessage);
                result?.Invoke(new GetMessagesResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                });
            });
        }

        // internal
        private void UpdateMessageList(MessageBody message)
        {
            string profileID = Profile.PlayerID;
            string reciverID = UsersIds.FirstOrDefault(x => x != profileID);
            string lastMessage = message.Body;

            FabChat.UpdateMessageList(new UpdateMessageListRequest { 
                SenderID = profileID,
                ReciverID = reciverID,
                LastMessage = lastMessage,
                RowKey = reciverID
            }, 
            onUpdate => { Debug.Log(onUpdate.FunctionResult); }, 
            onFailed => { Debug.LogError(onFailed.ErrorMessage); });
        }

        private string FixNickname(string nameText)
        {
            return string.IsNullOrEmpty(nameText) ? CBSConstants.UnknownName : nameText;
        }

        private string FixMessageLength(string bodyText)
        {
            string fixMessage = bodyText;
            int maxLength = ChatConfig.MaxMessageLength;
            int currentLength = fixMessage.Length;
            if (currentLength > maxLength)
            {
                fixMessage = fixMessage.Substring(0, maxLength) + "...";
            }
            return fixMessage;
        }

        private void StartNewMessageTask()
        {
            CompareIsRunning = true;
            CompareWorkTask();
        }

        private void StopNewMessageTask()
        {
            CompareIsRunning = false;
        }

        private async void CompareWorkTask()
        {
#if UNITY_WEBGL
            float startTime = Time.time;
            while (Time.time < startTime + CompareWait/1000f) await Task.Yield();
#else
            await Task.Delay(CompareWait);
#endif
            if (!CompareIsRunning)
            {
                return;
            }
            string tableId = CBSConstants.ChatTablePrefix + ChatID;
            FabAzure.GetDataFromTable(new AzureGetDataRequest
            {
                TableId = tableId,
                nTop = NewMessageCompareCount
            }, onGet =>
            {
                if (onGet.Error == null)
                {
                    var resultList = new List<MessageBody>();
                    var rawData = onGet.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var dataObject = jsonPlugin.DeserializeObject<TableResult>(rawData);
                    var messageList = dataObject.value;
                    foreach (var message in messageList)
                    {
                        bool isNew = CacheMessages == null ? false : CacheMessages.Count == 0 ? true : !CacheMessages.Any(x => x.ID == message.RowKey);
                        if (isNew)
                        {
                            var msg = JsonUtility.FromJson<MessageBody>(message.RawData);
                            msg.TimeStamp = message.Timestamp;
                            CacheMessages.Add(msg);
                            NewMessageAction?.Invoke(msg);
                        }
                    }

                    CompareWorkTask();
                }
                else
                {
                    CompareWorkTask();
                }
            }, onError => {
                CompareWorkTask();
            });
        }

        public void Dispose()
        {
            CacheMessages = null;
            StopNewMessageTask();
        }
    }

    [Serializable]
    public class MessageBody
    {
        private static CBSProfile profile;
        private static CBSProfile Profile
        {
            get
            {
                if (profile == null)
                {
                    profile = CBSModule.Get<CBSProfile>();
                }
                return profile;
            }
        }

        // serialize fields
        public string ID;
        public string Body;
        public string SenderID;
        public string SenderName;
        public string TimeStamp;

        // properties
        public bool IsMine
        {
            get
            {
                return SenderID == Profile.PlayerID;
            }
        }

        public static MessageBody SimpleFromText(string messageText)
        {
            return new MessageBody {
                ID = System.Guid.NewGuid().ToString(),
                Body = messageText,
                SenderID = Profile.PlayerID,
                SenderName = Profile.DisplayName
            };
        }
    }

    [Serializable]
    public class MessageListObject
    {
        public string PlayerID;
        public string PlayerName;
        public string LastMessage;
        public string TimeStamp;
    }

    public struct SendMessageResult
    {
        public bool IsSuccess;
        public SimpleError Error;
    }

    public struct GetMessagesResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public List<MessageBody> List;
    }

    public struct ChatRequest
    {
        public string ChatID;
        public ChatType Type;
        public string[] UsersIds;
        public int LoadAtStart;
    }

    public enum ChatType
    {
        GROUP,
        PRIVATE
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CBS.Playfab;
using PlayFab;
using UnityEngine;

namespace CBS
{
    public class CBSChat : CBSModule, ICBSChat
    {
        /// <summary>
        /// Notify when the current user has cleared the conversation from unread messages.
        /// </summary>
        public event Action<string> OnUnreadMessageClear;

        private Dictionary<string, ChatInstance> ChatCache { get; set; } = new Dictionary<string, ChatInstance>();

        private string ServerID { get; set; }
        private string RegionID { get; set; }

        private IProfile Profile { get; set; }
        private IFabAzure FabAzure { get; set; }

        private IFabChat FabChat { get; set; }

        protected override void Init()
        {
            Profile = Get<CBSProfile>();
            FabAzure = FabExecuter.Get<FabAzure>();
            FabChat = FabExecuter.Get<FabChat>();
        }

        /// <summary>
        /// Sets the server id for the server chat. You need to set this value before initializing the chat.
        /// </summary>
        /// <param name="serverID"></param>
        public void SetServerID(string serverID)
        {
            ServerID = serverID;
        }

        /// <summary>
        /// Sets the region ID for the regional chat. You need to set this value before initializing the chat. For example "ru", "en".
        /// </summary>
        /// <param name="region"></param>
        public void SetRegion(string region)
        {
            RegionID = region;
        }

        /// <summary>
        /// Get chat instance from pre-configured template. Global, Server, Regional.
        /// </summary>
        /// <param name="chatTitle"></param>
        /// <returns></returns>
        public ChatInstance GetOrCreateChat(ChatTitle chatTitle)
        {
            string chatID = GetChatID(chatTitle);
            bool exist = ChatCache.ContainsKey(chatID);
            var chatRequest = new ChatRequest
            {
                ChatID = chatID,
                LoadAtStart = CBSConstants.MaxChatHistory,
                Type = ChatType.GROUP
            };
            return exist ? ChatCache[chatID] : new ChatInstance(chatRequest);
        }

        /// <summary>
        /// Get chat instance from custom id. Suitable for creating group chats for example.
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        public ChatInstance GetOrCreateChatByID(string chatID)
        {
            var chatRequest = new ChatRequest
            {
                ChatID = chatID,
                LoadAtStart = CBSConstants.MaxChatHistory,
                Type = ChatType.GROUP
            };
            bool exist = ChatCache.ContainsKey(chatID);
            return exist ? ChatCache[chatID] : new ChatInstance(chatRequest);
        }

        /// <summary>
        /// Get private chat instance from custom id. Suitable for private chats, from lists of unread messages.
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        public ChatInstance GetOrCreateChatWithUser(string userID)
        {
            string profileID = Profile.PlayerID;
            string [] userIds = new string[] { userID, profileID };
            Array.Sort(userIds);
            string chatID = userIds[0] + userIds[1];

            var chatRequest = new ChatRequest
            {
                ChatID = chatID,
                LoadAtStart = CBSConstants.MaxChatHistory,
                Type = ChatType.PRIVATE,
                UsersIds = userIds
            };
            bool exist = ChatCache.ContainsKey(userID);
            return exist ? ChatCache[userID] : new ChatInstance(chatRequest);
        }

        /// <summary>
        /// Get list of main chat titles.
        /// </summary>
        /// <returns></returns>
        public List<ChatTitle> GetChatTitles()
        {
            return Enum.GetValues(typeof(ChatTitle)).Cast<ChatTitle>().ToList();
        }

        /// <summary>
        /// Get a list of the current player's conversations with whom there was previously a private conversation.
        /// </summary>
        /// <param name="result"></param>
        public void GetUserDialogList(Action<GetDialogListResult> result)
        {
            string profileID = Profile.PlayerID;
            string dialogPrefix = CBSConstants.ChatListTablePrefix;
            string tableID = dialogPrefix + profileID;
            FabAzure.GetDataFromTable(new AzureGetDataRequest {
                TableId = tableID
            }, onGet => { 
                if (onGet.Error == null)
                {
                    var rawData = onGet.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var resulrObject = jsonPlugin.DeserializeObject<MesasgeDislogCallback>(rawData);
                    resulrObject.value = resulrObject.value.OrderByDescending(x => long.Parse(string.IsNullOrEmpty(x.UpdateTime) ? "0" : x.UpdateTime)).ToList();
                    result?.Invoke(new GetDialogListResult
                    {
                        IsSuccess = true,
                        Dialogs = resulrObject.value
                    });
                }
                else
                {
                    result?.Invoke(new GetDialogListResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
            }, onFailed => {
                result?.Invoke(new GetDialogListResult { 
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Clears the list of unread messages with a specific user.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="result"></param>
        public void ClearUnreadMessageWithUser(string userID, Action<ClearUnreadMessageResult> result)
        {
            string profileID = Profile.PlayerID;
            FabChat.ClearUnreadMessage(profileID, userID, onClear => {
                if (onClear.Error != null)
                {
                    Debug.Log(onClear.Error.Message);
                    result?.Invoke(new ClearUnreadMessageResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onClear.Error)
                    });
                }
                else
                {
                    result?.Invoke(new ClearUnreadMessageResult
                    {
                        IsSuccess = true
                    });
                    OnUnreadMessageClear?.Invoke(userID);
                }
            }, onFailed => {
                result?.Invoke(new ClearUnreadMessageResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
            
        }

        // internal
        private string GetChatID(ChatTitle chatTitle)
        {
            switch (chatTitle)
            {
                case ChatTitle.GLOBAL:
                    return CBSConstants.ChatGlobalID;
                case ChatTitle.REGIONAL:
                    return string.IsNullOrEmpty(RegionID) ? CBSConstants.ChatDefaultRegion : RegionID;
                case ChatTitle.SERVER:
                    return string.IsNullOrEmpty(ServerID) ? CBSConstants.ChatDefaultServer : ServerID;
            }
            return string.Empty;
        }

        protected override void OnLogout()
        {
            foreach (var keyPair in ChatCache)
            {
                keyPair.Value.Dispose();
            }
            ChatCache = new Dictionary<string, ChatInstance>();
        }
    }

    [Serializable]
    public class MesasgeDislogCallback
    {
        public List<MessageDialogObject> value;
    }

    [Serializable]
    public class MessageDialogObject
    {
        public string PartitionKey;
        public string RowKey;
        public string Timestamp;
        public string LastMessage;
        public string UnreadCount;
        public string UpdateTime;
        public string UserID;
        public string UserName;
    }

    public struct GetDialogListResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public List<MessageDialogObject> Dialogs;
    }

    public struct ClearUnreadMessageResult
    {
        public bool IsSuccess;
        public SimpleError Error;
    }

    public enum ChatTitle
    {
        GLOBAL,
        SERVER,
        REGIONAL
    }
}

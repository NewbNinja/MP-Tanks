using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS
{
    public interface ICBSChat
    {
        /// <summary>
        /// Notify when the current user has cleared the conversation from unread messages.
        /// </summary>
        event Action<string> OnUnreadMessageClear;

        /// <summary>
        /// Sets the server id for the server chat. You need to set this value before initializing the chat.
        /// </summary>
        /// <param name="serverID"></param>
        void SetServerID(string serverID);

        /// <summary>
        /// Sets the region ID for the regional chat. You need to set this value before initializing the chat. For example "ru", "en".
        /// </summary>
        /// <param name="region"></param>
        void SetRegion(string region);

        /// <summary>
        /// Get chat instance from pre-configured template. Global, Server, Regional.
        /// </summary>
        /// <param name="chatTitle"></param>
        /// <returns></returns>
        ChatInstance GetOrCreateChat(ChatTitle chatTitle);

        /// <summary>
        /// Get chat instance from custom id. Suitable for creating group chats for example.
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        ChatInstance GetOrCreateChatByID(string chatID);

        /// <summary>
        /// Get private chat instance from custom id. Suitable for private chats, from lists of unread messages.
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        ChatInstance GetOrCreateChatWithUser(string chatID);

        /// <summary>
        /// Get list of main chat titles.
        /// </summary>
        /// <returns></returns>
        List<ChatTitle> GetChatTitles();

        /// <summary>
        /// Get a list of the current player's conversations with whom there was previously a private conversation.
        /// </summary>
        /// <param name="result"></param>
        void GetUserDialogList(Action<GetDialogListResult> result);

        /// <summary>
        /// Clears the list of unread messages with a specific user.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="result"></param>
        void ClearUnreadMessageWithUser(string userID, Action<ClearUnreadMessageResult> result);
    }
}

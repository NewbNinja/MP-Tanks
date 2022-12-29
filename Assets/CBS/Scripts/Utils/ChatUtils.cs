using CBS.Scriptable;
using CBS.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Utils
{
    public static class ChatUtils
    {
        public static string ConvertNickname(string inputName)
        {
            return "[<b>" + inputName + "</b>]";
        }

        public static string MarkAsMine(string inputText)
        {
            return "<color=orange>" + inputText + "</color>";
        }

        public static Color GetMineBubbleColor()
        {
            return new Color(159f / 255f, 255f / 255f , 150f / 255f);
        }

        public static void ShowSimpleChat(ChatInstance chat)
        {
            var prefabs = CBSScriptable.Get<ChatPrefabs>();
            var chatPrefab = prefabs.SimpleChatView;
            var activeChatInstance = UIView.GetInstance(chatPrefab);
            if (activeChatInstance != null)
            {
                UIView.HideWindow(activeChatInstance);
            }
            activeChatInstance = UIView.ShowWindow(chatPrefab);
            activeChatInstance.GetComponent<ChatView>().Init(chat);
        }
    }
}

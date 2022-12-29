using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "ChatPrefabs", menuName = "CBS/Add new Chat Prefabs")]
    public class ChatPrefabs : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/ChatPrefabs";

        public GameObject ChatWindow;
        public GameObject SimpleChatMessage;
        public GameObject BubbleChatMessage;
        public GameObject ClanChatMessage;
        public GameObject SimpleChatView;
    }
}

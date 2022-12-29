using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class ClanChatView : MonoBehaviour, ISetClan
    {
        [SerializeField]
        private ChatView ChatView;

        private ICBSChat cbsChat;
        private ICBSChat CBSChat {
            get {
                if (cbsChat == null)
                    cbsChat = CBSModule.Get<CBSChat>();
                return cbsChat;
            }
        }
        private ChatPrefabs prefabs { get; set; }
        protected ChatPrefabs Prefabs
        {
            get
            {
                prefabs = prefabs ?? CBSScriptable.Get<ChatPrefabs>();
                return prefabs;
            }
        }

        public string ClanID { get; private set; }

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(ClanID))
                return;
            Setup(ClanID);
        }

        public void Setup(string clanID)
        {
            ClanID = clanID;
            var activeChat = CBSChat.GetOrCreateChatByID(clanID);
            ChatView.Init(activeChat);
        }

        public void SetClanID(string clanID)
        {
            ClanID = clanID;
            ChatView.SetMessagePrefab(Prefabs.ClanChatMessage);
        }
    }
}

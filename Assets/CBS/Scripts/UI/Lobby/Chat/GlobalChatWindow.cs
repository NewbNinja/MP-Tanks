using CBS.Core;
using CBS.Scriptable;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class GlobalChatWindow : MonoBehaviour
    {
        [SerializeField]
        private ChatView ChatUI;
        [SerializeField]
        private ToggleGroup CategoryGroup;
        [SerializeField]
        private Transform TitlesRoot;

        private ICBSChat CBSChat { get; set; }

        private CommonPrefabs Prefabs { get; set; }

        private ChatTitle SelectedChat { get; set; }

        private ChatInstance ActiveChat { get; set; }

        private void Awake()
        {
            CBSChat = CBSModule.Get<CBSChat>();
            Prefabs = CBSScriptable.Get<CommonPrefabs>();
            DisplayChatTitles();
        }

        public void OnEnable()
        {
            if (ActiveChat != null)
            {
                ChatUI.Init(ActiveChat);
            }
        }

        private void DisplayChatTitles()
        {
            var titles = CBSChat.GetChatTitles();
            // add listeners
            for (int i=0;i< titles.Count;i++)
            {
                var title = titles[i];
                var tabPrefab = Prefabs.BaseTab;
                var tabObject = Instantiate(tabPrefab, TitlesRoot);
                tabObject.GetComponent<Toggle>().group = CategoryGroup;
                var tabComponent = tabObject.GetComponent<BaseTab<string>>();
                tabComponent.TabObject = title.ToString();
                tabComponent.SetSelectAction(OnTitleSelected);
                if (i == 0)
                {
                    tabObject.GetComponent<Toggle>().isOn = true;
                }
            }
        }

        private void OnTitleSelected(string title)
        {
            SelectedChat = (ChatTitle)Enum.Parse(typeof(ChatTitle), title, true);
            ActiveChat = CBSChat.GetOrCreateChat(SelectedChat);
            ChatUI.Init(ActiveChat);
        }
    }
}

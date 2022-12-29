using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace CBS.UI
{
    public class LeaderboardTabListener : MonoBehaviour
    {
        public event Action<LeaderboardTabType> OnTabSelected;

        [SerializeField]
        private GameObject[] AllTabs;

        public LeaderboardTabType ActiveTab { get; private set; }

        private void Awake()
        {
            foreach (var tab in AllTabs)
            {
                tab.GetComponent<Toggle>().onValueChanged.AddListener(OnToggleSelected);
            }
        }

        private void OnDestroy()
        {
            foreach (var tab in AllTabs)
            {
                tab.GetComponent<Toggle>().onValueChanged.RemoveListener(OnToggleSelected);
            }
        }

        private void OnToggleSelected(bool val)
        {
            if (val)
            {
                var activeTab = AllTabs.FirstOrDefault(x => x.GetComponent<Toggle>().isOn);
                ActiveTab = activeTab.GetComponent<LeaderboardTab>().GetTabType();
                OnTabSelected?.Invoke(ActiveTab);
            }
        }
    }
}
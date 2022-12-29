using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace CBS.UI
{
    public class AchievementsTabListener : MonoBehaviour
    {
        public event Action<AchievementsTabType> OnTabSelected;

        [SerializeField]
        private GameObject[] AllTabs;

        public AchievementsTabType ActiveTab { get; private set; }

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
                ActiveTab = activeTab.GetComponent<AchievementsTab>().GetTabType();
                OnTabSelected?.Invoke(ActiveTab);
            }
        }
    }
}
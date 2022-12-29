using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class AchievementsWindow : MonoBehaviour
    {
        [SerializeField]
        private TasksScroller Scroller;
        [SerializeField]
        private AchievementsTabListener TabListener;

        private IAchievements Achievements { get; set; }
        private AchievementsPrefabs Prefabs { get; set; }

        private void Awake()
        {
            Achievements = CBSModule.Get<CBSAchievements>();
            Prefabs = CBSScriptable.Get<AchievementsPrefabs>();
            TabListener.OnTabSelected += OnTabSelected;
        }

        private void OnDestroy()
        {
            TabListener.OnTabSelected -= OnTabSelected;
        }

        private void OnEnable()
        {
            var activeTab = TabListener.ActiveTab;
            GetAchievements(activeTab);
        }

        private void OnTabSelected(AchievementsTabType tab)
        {
            GetAchievements(tab);
        }

        private void GetAchievements(AchievementsTabType tab)
        {
            Scroller.HideAll();
            if (tab == AchievementsTabType.ALL)
                Achievements.GetAchievementsTable(OnGetAchievements);
            else if (tab == AchievementsTabType.ACTIVE)
                Achievements.GetActiveAchievementsTable(OnGetAchievements);
            else if (tab == AchievementsTabType.COMPLETED)
                Achievements.GetCompletedAchievementsTable(OnGetAchievements);
        }

        private void OnGetAchievements(GetAchievementsTableResult result)
        {
            if (result.IsSuccess)
            {
                var achievements = result.AchievementsData;
                var achievementsList = achievements.Achievements;
                var achievementPrefab = Prefabs.AchievementsSlot;

                Scroller.Spawn(achievementPrefab, achievementsList);
            }
            else
            {
                new PopupViewer().ShowFabError(result.Error);
                gameObject.SetActive(false);
            }
        }
    }
}

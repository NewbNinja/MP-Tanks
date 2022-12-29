using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CBS.UI
{
    public class AchievementsBadge : BaseBadge
    {
        private IAchievements CBSAchievements { get; set; }

        private void Awake()
        {
            CBSAchievements = CBSModule.Get<CBSAchievements>();

            CBSAchievements.OnCompleteAchievement += OnCompleteAchievement;
            CBSAchievements.OnPlayerRewarded += OnPlayerRewarded;

            UpdateCount(0);
        }

        private void OnDestroy()
        {
            CBSAchievements.OnCompleteAchievement -= OnCompleteAchievement;
            CBSAchievements.OnPlayerRewarded -= OnPlayerRewarded;
        }

        private void OnEnable()
        {
            GetCompleteAchievements();
        }

        private void GetCompleteAchievements()
        {
            CBSAchievements.GetCompletedAchievementsTable(OnGetAchievements);
        }

        private void OnGetAchievements(GetAchievementsTableResult result)
        {
            if (result.IsSuccess)
            {
                var achievements = result.AchievementsData.Achievements;
                var notRewardedAchievements = achievements.Where(x => x.Reward != null && x.Rewarded == false).Count();
                UpdateCount(notRewardedAchievements);
            }
        }

        private void OnPlayerRewarded(PrizeObject obj)
        {
            GetCompleteAchievements();
        }

        private void OnCompleteAchievement(CompleteAchievementResult obj)
        {
            GetCompleteAchievements();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CBS.UI
{
    public class DailyTasksBadge : BaseBadge
    {
        private IDailyTasks CBSDailyTasks { get; set; }

        private void Awake()
        {
            CBSDailyTasks = CBSModule.Get<CBSDailyTasks>();

            CBSDailyTasks.OnCompleteTask += OnCompleteAchievement;
            CBSDailyTasks.OnPlayerRewarded += OnPlayerRewarded;
            CBSDailyTasks.OnTaskReseted += OnTaskReseted;

            UpdateCount(0);
        }

        private void OnDestroy()
        {
            CBSDailyTasks.OnCompleteTask -= OnCompleteAchievement;
            CBSDailyTasks.OnPlayerRewarded -= OnPlayerRewarded;
            CBSDailyTasks.OnTaskReseted -= OnTaskReseted;
        }

        private void OnEnable()
        {
            GetCompleteTasks();
        }

        private void GetCompleteTasks()
        {
            CBSDailyTasks.GetPlayerDailyTasks(OnGetPlayerTasks);
        }

        private void OnGetPlayerTasks(GetPlayerDailyTasksResult result)
        {
            if (result.IsSuccess)
            {
                var tasks = result.CurrentTasks;
                var notRewardedTasks = tasks.Where(x => x.Reward != null && x.Rewarded == false && x.IsComplete).Count();
                UpdateCount(notRewardedTasks);
            }
        }

        private void OnPlayerRewarded(PrizeObject reward)
        {
            GetCompleteTasks();
        }

        private void OnCompleteAchievement(CompleteTaskResult result)
        {
            GetCompleteTasks();
        }

        private void OnTaskReseted(GetPlayerDailyTasksResult result)
        {
            GetCompleteTasks();
        }
    }
}

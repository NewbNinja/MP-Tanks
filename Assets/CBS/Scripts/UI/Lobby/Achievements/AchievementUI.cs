using CBS.Core;
using CBS.Scriptable;
using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class AchievementUI : BaseTaskUI, IScrollableItem<CBSTask>
    {
        protected override string LockText => AchievementsTXTHandler.GetLockText(Task.LockLevel);
        protected override string NotCompleteText => AchievementsTXTHandler.NotCompleteText;
        protected override string CompleteText => AchievementsTXTHandler.CompleteText;

        private IAchievements Achievements { get; set; }

        protected override void OnInit()
        {
            base.OnInit();
            Achievements = CBSModule.Get<CBSAchievements>();
        }

        // button click
        public override void OnAddPoint()
        {
            var achievementID = Task.ID;
            Achievements.AddAchievementPoint(achievementID, onAdd => {
                if (onAdd.IsSuccess)
                {
                    var updatedAchievement = onAdd.Achievement;
                    Display(updatedAchievement);
                }
            });
        }

        public void ResetAchievement()
        {
            var achievementID = Task.ID;
            Achievements.ResetAchievement(achievementID, onReset => {
                if (onReset.IsSuccess)
                {
                    var updatedAchievement = onReset.Achievement;
                    Display(updatedAchievement);
                }
            });
        }

        public override void GetRewards()
        {
            var achievementID = Task.ID;
            Achievements.PickupAchievementReward(achievementID, onPick => {
                if (onPick.IsSuccess)
                {
                    var updatedAchievement = onPick.Achievement;
                    Display(updatedAchievement);
                }
            });
        }
    }
}

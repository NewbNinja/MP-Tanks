using CBS.Core;
using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class DailyTasksUI : BaseTaskUI, IScrollableItem<CBSTask>
    {
        protected override string LockText => TasksTXTHandler.GetLockText(Task.LockLevel);
        protected override string NotCompleteText => TasksTXTHandler.NotCompleteText;
        protected override string CompleteText => TasksTXTHandler.CompleteText;

        private IDailyTasks DailyTask { get; set; }

        protected override void OnInit()
        {
            base.OnInit();
            DailyTask = CBSModule.Get<CBSDailyTasks>();
        }

        public override void OnAddPoint()
        {
            var taskID = Task.ID;
            DailyTask.AddTaskPoint(taskID, onAdd => {
                if (onAdd.IsSuccess)
                {
                    var updatedTask = onAdd.Task;
                    Display(updatedTask);
                }
            });
        }

        public override void GetRewards()
        {
            var taskID = Task.ID;
            DailyTask.PickupTaskReward(taskID, onPick => {
                if (onPick.IsSuccess)
                {
                    var updatedTask = onPick.Task;
                    Display(updatedTask);
                }
            });
        }
    }
}

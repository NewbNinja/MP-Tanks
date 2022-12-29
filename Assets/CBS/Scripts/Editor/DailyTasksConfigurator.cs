#if ENABLE_PLAYFABADMIN_API
using CBS.Editor.Window;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CBS.Editor
{
    public class DailyTasksConfigurator : BaseTasksConfigurator<CBSTask, DailyTasksData, AddDailyTaskWindow>
    {
        protected override string Title => "Daily Tasks";

        protected override string TASK_TITLE_ID => CBSConstants.DailyTasksTitleKey;

        protected override string[] Titles => new string[] { "Tasks", "Additional configs" };

        protected override string ItemKey => "task";

        private int DailyTasksCount { get; set; }

        protected override void DrawConfigs()
        {
            base.DrawConfigs();

            GUILayout.Space(15);
            DailyTasksCount = TasksData.DailyTasksCount;
            DailyTasksCount = EditorGUILayout.IntField("Daily Tasks Count", DailyTasksCount);
            EditorGUILayout.HelpBox("The number of tasks available to the player every day. Cannot be less than 1", MessageType.Info);
            if (DailyTasksCount < 1)
                DailyTasksCount = 1;
            TasksData.DailyTasksCount = DailyTasksCount;
        }
    }
}
#endif

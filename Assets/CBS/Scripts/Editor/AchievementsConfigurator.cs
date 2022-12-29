#if ENABLE_PLAYFABADMIN_API
using CBS.Editor.Window;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Editor
{
    public class AchievementsConfigurator : BaseTasksConfigurator<CBSTask, AchievementsData, AddAchievementWindow>
    {
        protected override string Title => "Achievements";

        protected override string TASK_TITLE_ID => CBSConstants.AchievementsTitleKey;

        protected override string[] Titles => new string[] { "Achievements", "Additional configs" };

        protected override string ItemKey => "achievement";
    }
}
#endif

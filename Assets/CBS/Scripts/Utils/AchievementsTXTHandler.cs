using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Utils
{
    public class AchievementsTXTHandler
    {
        public const string NotCompleteText = "Achievement not yet completed";
        public const string CompleteText = "Achievement completed!";

        public static string GetLockText(int level)
        {
            return string.Format("Achievement is available at the level {0}", level);
        }
    }
}

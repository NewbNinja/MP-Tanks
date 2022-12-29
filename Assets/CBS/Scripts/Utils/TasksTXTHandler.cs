using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Utils
{
    public class TasksTXTHandler
    {
        public const string NotCompleteText = "Task not yet completed";
        public const string CompleteText = "Task completed!";

        public static string GetLockText(int level)
        {
            return string.Format("Task is available at the level {0}", level);
        }
    }
}

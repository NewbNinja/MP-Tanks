using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "DailyTasksPrefabs", menuName = "CBS/Add new Daily Tasks Prefabs")]
    public class DailyTasksPrefabs : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/DailyTasksPrefabs";

        public GameObject DailyTasksWindow;
        public GameObject TaskSlot;
    }
}

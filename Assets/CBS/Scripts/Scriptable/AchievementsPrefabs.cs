using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "AchievementsPrefabs", menuName = "CBS/Add new Achievements Prefabs")]
    public class AchievementsPrefabs : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/AchievementsPrefabs";

        public GameObject AchievementsWindow;
        public GameObject AchievementsSlot;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "BattlePassPrefabs", menuName = "CBS/Add new Battle Pass Prefabs")]
    public class BattlePassPrefabs : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/BattlePassPrefabs";

        public GameObject BattlePassFrame;
        public GameObject BattlePassWindow;
        public GameObject RewardDrawer;
        public GameObject LevelDrawer;
    }
}

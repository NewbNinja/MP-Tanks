using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "DailyBonusPrefabs", menuName = "CBS/Add new DailyBonus Prefabs")]
    public class DailyBonusPrefabs : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/DailyBonusPrefabs";

        public GameObject DailyBonusWindow;
        public GameObject DailyBonusSlot;
    }
}

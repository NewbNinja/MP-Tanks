using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "LootboxPrefabs", menuName = "CBS/Add new Lootbox Prefabs")]
    public class LootboxPrefabs : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/LootboxPrefabs";

        public GameObject LootBoxes;
        public GameObject LootBoxSlot;
        public GameObject LootBoxResult;
    }
}

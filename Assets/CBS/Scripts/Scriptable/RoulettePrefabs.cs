using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "RoulettePrefabs", menuName = "CBS/Add new Roulette Prefabs")]
    public class RoulettePrefabs : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/RoulettePrefabs";

        public GameObject RouletteWindow;
        public GameObject RouletteSlot;
    }
}

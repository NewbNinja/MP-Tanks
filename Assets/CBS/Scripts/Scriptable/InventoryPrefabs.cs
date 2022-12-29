using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "InventoryPrefabs", menuName = "CBS/Add new Inventory Prefabs")]
    public class InventoryPrefabs : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/InventoryPrefabs";

        public GameObject Inventory;
        public GameObject InventorySlot;
    }
}

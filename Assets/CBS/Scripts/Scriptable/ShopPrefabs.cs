using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "ShopPrefabs", menuName = "CBS/Add new Shop Prefabs")]
    public class ShopPrefabs : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/ShopPrefabs";

        public GameObject ItemsShop;
        public GameObject CategoryTab;
        public GameObject ShopItem;
        public GameObject ShopPack;
        public GameObject ShopLootBox;
    }
}

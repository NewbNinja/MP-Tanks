using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "CommonPrefabs", menuName = "CBS/Add new Common Prefabs")]
    public class CommonPrefabs : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/CommonPrefabs";

        public GameObject Canvas;
        public GameObject BaseTab;
        public GameObject IconsPanel;
        public GameObject PurchaseButton;
        public GameObject SimpleIcon;
        public GameObject GameContext;
    }
}

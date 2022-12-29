using CBS.Scriptable;
using CBS.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Context
{
    public class GameContext : MonoBehaviour
    {
        private CommonPrefabs Prefabs { get; set; }

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            Prefabs = CBSScriptable.Get<CommonPrefabs>();

            var prefab = Prefabs.GameContext;
            UIView.ShowWindow(prefab);
        }
    }
}

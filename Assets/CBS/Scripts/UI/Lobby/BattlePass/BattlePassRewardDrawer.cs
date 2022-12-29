using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class BattlePassRewardDrawer : PrizeDrawer
    {
        private BattlePassPrefabs PassPrefabs { get; set; }

        protected override GameObject RewardPrefab => PassPrefabs.RewardDrawer;

        protected override void Init()
        {
            base.Init();
            PassPrefabs = CBSScriptable.Get<BattlePassPrefabs>();
        }
    }
}

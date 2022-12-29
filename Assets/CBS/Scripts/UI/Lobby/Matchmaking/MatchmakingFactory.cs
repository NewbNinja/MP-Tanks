using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class MatchmakingFactory : IMatchmakingFactory
    {
        private MatchmakingPrefabs Prefabs { get; set; }

        public MatchmakingFactory ()
        {
            Prefabs = CBSScriptable.Get<MatchmakingPrefabs>();
        }

        public GameObject SpawnMatchmakingResult(CBSMatchmakingQueue queue)
        {
            if (queue.Mode == MatchmakingMode.Single)
            {
                var singlePrefab = queue.IsDuel() ? Prefabs.DuelPreview : Prefabs.SinglePreview;
                return UIView.ShowWindow(singlePrefab);
            }
            else if (queue.Mode == MatchmakingMode.Team)
            {
                var teamPrefab = Prefabs.TeamPreview;
                return UIView.ShowWindow(teamPrefab);
            }
            return null;
        }
    }
}

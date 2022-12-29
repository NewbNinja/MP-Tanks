using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CBS.Scriptable;

namespace CBS.UI
{
    public class PlayerLeaderboard : MonoBehaviour
    {
        [SerializeField]
        private PlayerLeaderboardScroller Scroller;

        protected ILeaderboard Leaderboard { get; set; }

        private LeaderboardPrefabs Prefabs { get; set; }

        private void Awake()
        {
            Leaderboard = CBSModule.Get<CBSLeaderboard>();
            Prefabs = CBSScriptable.Get<LeaderboardPrefabs>();
        }

        private void OnEnable()
        {
            Scroller.HideAll();
            GetPlayersLeaderboard();
        }

        protected virtual void GetPlayersLeaderboard()
        {
            Leaderboard.GetLevelLeadearboard(OnGetLeaderboard);
        }

        protected void OnGetLeaderboard(GetLeaderboardResult result)
        {
            if (result.IsSuccess)
            {
                var leaderboad = result.Leaderboards;
                string profileID = result.ProfileResult.PlayFabId;
                bool existInTop = leaderboad.Any(x => x.PlayFabId == profileID);
                if (!existInTop && !string.IsNullOrEmpty(profileID))
                {
                    leaderboad.Add(result.ProfileResult);
                }

                var prefab = Prefabs.LeaderboardUser;
                Scroller.Spawn(prefab, leaderboad);
            }
        }
    }
}

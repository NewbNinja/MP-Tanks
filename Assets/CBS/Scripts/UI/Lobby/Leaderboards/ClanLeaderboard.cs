using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS.UI
{
    public class ClanLeaderboard : MonoBehaviour
    {
        [SerializeField]
        private ClanLeaderboardScroller Scroller;

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
            Leaderboard.GetClansLeaderboard(OnGetLeaderboard);
        }

        protected void OnGetLeaderboard(GetClanLeaderboardResult result)
        {
            if (result.IsSuccess)
            {
                var leaderboad = result.Leaderboards;
                string clanID = result.ProfileResult == null ? string.Empty : result.ProfileResult.ClanId;
                if (!string.IsNullOrEmpty(clanID))
                {
                    bool existInTop = leaderboad.Any(x => x.ClanId == clanID);
                    if (!existInTop)
                    {
                        leaderboad.Add(result.ProfileResult);
                    }
                }

                var prefab = Prefabs.LeaderboardClan;
                Scroller.Spawn(prefab, leaderboad);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CBS.Scriptable;

namespace CBS.UI
{
    public class TournamentLeaderboard : MonoBehaviour
    {
        [SerializeField]
        private PlayerTournamentScroller Scroller;

        private TournamentPrefabs Prefabs { get; set; }

        private void Awake()
        {
            Prefabs = CBSScriptable.Get<TournamentPrefabs>();
        }

        public void DisplayLeaderboard(GetTournamentStateResult result)
        {
            if (result.IsSuccess)
            {
                var leaderboad = result.Leaderboard;

                var prefab = Prefabs.TournamentUser;
                Scroller.Spawn(prefab, leaderboad);
            }
        }

        public void Clear()
        {
            Scroller.HideAll();
        }
    }
}

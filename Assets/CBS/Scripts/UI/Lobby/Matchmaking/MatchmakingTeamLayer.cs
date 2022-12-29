using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI.Matchmaking
{
    public class MatchmakingTeamLayer : MonoBehaviour
    {
        [SerializeField]
        private Text TeamLabel;

        private MatchmakingPrefabs Prefabs { get; set; }

        private void Awake()
        {
            Prefabs = CBSScriptable.Get<MatchmakingPrefabs>();
        }

        public void DrawTeam(List<CBSMatchmakingPlayer> list, string teamID)
        {
            TeamLabel.text = teamID;
            var playerPrefab = Prefabs.SinglePlayer;

            foreach (var player in list)
            {
                var playerUI = Instantiate(playerPrefab, transform);
                playerUI.GetComponent<MatchmakingPreviewPlayer>().DrawUser(player);
            }
        }
    }
}

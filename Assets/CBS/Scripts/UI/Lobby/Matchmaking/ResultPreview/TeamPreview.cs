using CBS.Scriptable;
using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI.Matchmaking
{
    public class TeamPreview : MonoBehaviour, IMatchmakingPreview
    {
        [SerializeField]
        private Transform Root;

        private MatchmakingPrefabs Prefabs { get; set; }

        private Dictionary<string, List<CBSMatchmakingPlayer>> TeamList;

        private void Awake()
        {
            Prefabs = CBSScriptable.Get<MatchmakingPrefabs>();
            
        }

        public void Draw(StartMatchResult result)
        {
            TeamList = new Dictionary<string, List<CBSMatchmakingPlayer>>();

            var players = result.Players;

            foreach (var player in players)
            {
                var teamID = player.Team;
                if (TeamList.ContainsKey(teamID))
                {
                    TeamList[teamID].Add(player);
                }
                else
                {
                    TeamList[teamID] = new List<CBSMatchmakingPlayer>();
                    TeamList[teamID].Add(player);
                }
            }

            var teamPrefab = Prefabs.TeamLayer;
            Root.Clear();

            foreach (var team in TeamList)
            {
                var playerUI = Instantiate(teamPrefab, Root);
                playerUI.GetComponent<MatchmakingTeamLayer>().DrawTeam(team.Value, team.Key);
            }
        }

        // button click
        public void StartMatch()
        {
            gameObject.SetActive(false);
        }
    }
}

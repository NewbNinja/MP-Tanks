using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "LeaderboardPrefabs", menuName = "CBS/Add new Leaderboard Prefabs")]
    public class LeaderboardPrefabs : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/LeaderboardPrefabs";

        public GameObject LeaderboardsWindow;
        public GameObject LeaderboardUser;
        public GameObject LeaderboardClan;
    }
}

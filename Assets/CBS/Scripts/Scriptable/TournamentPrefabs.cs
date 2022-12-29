using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "TournamentPrefabs", menuName = "CBS/Add new Tournament Prefabs")]
    public class TournamentPrefabs : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/TournamentPrefabs";

        public GameObject TournamentWindow;
        public GameObject TournamentUser;
        public GameObject TournamentFinish;
    }
}

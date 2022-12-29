using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "ClanPrefabs", menuName = "CBS/Add new Clan Prefabs")]
    public class ClanPrefabs : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/ClanPrefabs";

        public GameObject ClanWindow;
        public GameObject NoClanWindow;
        public GameObject ClanSearchResult;
        public GameObject ClanInviteResult;
        public GameObject ClanRequestedUser;
        public GameObject ClanUser;
    }
}

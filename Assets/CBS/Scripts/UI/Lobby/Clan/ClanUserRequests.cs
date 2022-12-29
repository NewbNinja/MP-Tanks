using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS.UI
{
    public class ClanUserRequests : MonoBehaviour, ISetClan
    {
        [SerializeField]
        private ClanRequestedUserScroller Scroller;

        private IClan CBSClan { get; set; }
        private ClanPrefabs Prefabs { get; set; }

        public string ClanID { get; private set; }

        public void SetClanID(string clanID)
        {
            ClanID = clanID;
        }

        private void Awake()
        {
            CBSClan = CBSModule.Get<CBSClan>();
            Prefabs = CBSScriptable.Get<ClanPrefabs>();
        }

        private void OnEnable()
        {
            Scroller.HideAll();
            CBSClan.GetClanUsersJoinRequests(ClanID, OnGetUsers);
        }

        private void OnGetUsers(GetClanRequestedUsersResult result)
        {
            if (result.IsSuccess)
            {
                var profilePrefab = Prefabs.ClanRequestedUser;
                var profiles = result.Profiles.ToList();
                Scroller.Spawn(profilePrefab, profiles);
            }
        }
    }
}

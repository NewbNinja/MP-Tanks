using CBS.Core;
using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class UserInvitations : MonoBehaviour
    {
        [SerializeField]
        private BaseScroller Scroller;

        private IClan CBSClan { get; set; }

        private List<InvitationInfo> Invitations { get; set; }

        private ClanPrefabs Prefabs { get; set; }

        private void Awake()
        {
            Prefabs = CBSScriptable.Get<ClanPrefabs>();
            CBSClan = CBSModule.Get<CBSClan>();
            Scroller.OnSpawn += OnClanSpawn;
        }

        private void OnDestroy()
        {
            Scroller.OnSpawn -= OnClanSpawn;
        }

        private void OnEnable()
        {
            Scroller.Clear();
            CBSClan.GetUserInvitations(OnGetInvitations);
        }

        private void OnGetInvitations(GetUserInvatitaionsResult result)
        {
            if (result.IsSuccess)
            {
                var itemPrefab = Prefabs.ClanInviteResult;
                Invitations = result.Invites;
                Scroller.SpawnItems(itemPrefab, Invitations.Count);
            }
        }

        private void OnClanSpawn(GameObject uiItem, int index)
        {
            var clan = Invitations[index];
            uiItem.GetComponent<ClanInvationResult>().Display(clan);
        }
    }
}

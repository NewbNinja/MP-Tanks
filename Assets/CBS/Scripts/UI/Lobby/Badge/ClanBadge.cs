using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class ClanBadge : BaseBadge
    {
        private IClan CBSClan { get; set; }
        private IProfile CBSProfile { get; set; }

        private void Awake()
        {
            CBSProfile = CBSModule.Get<CBSProfile>();
            CBSClan = CBSModule.Get<CBSClan>();

            Back.SetActive(false);

            // add listeners
            CBSClan.OnCreateClan += OnUserCreateClan;
            CBSClan.OnLeaveClan += OnUserLeaveClan;
            CBSClan.OnJoinClan += OnUserJoinClan;
            CBSClan.OnRemoveClan += OnUserRemoveClan;
            CBSClan.OnUserAccepted += OnUserAccepted;
            CBSClan.OnUserDeclined += OnUserDeclined;
            CBSClan.OnUserDeclineClanInvation += OnUserDeclineClanInvation;
        }

        private void OnDestroy()
        {
            // remove listeners
            CBSClan.OnCreateClan -= OnUserCreateClan;
            CBSClan.OnLeaveClan -= OnUserLeaveClan;
            CBSClan.OnJoinClan -= OnUserJoinClan;
            CBSClan.OnRemoveClan -= OnUserRemoveClan;
            CBSClan.OnUserAccepted -= OnUserAccepted;
            CBSClan.OnUserDeclined -= OnUserDeclined;
            CBSClan.OnUserDeclineClanInvation -= OnUserDeclineClanInvation;
        }

        private void OnEnable()
        {
            CheckState();
        }

        private void CheckState()
        {
            UpdateCount(0);
            CBSClan.ExistInClan(onCheck => {
                if (onCheck.IsSuccess)
                {
                    if (onCheck.ExistInClan)
                        GetClanUserRequests(onCheck.ClanID);
                    else
                        GetClanInvitationBadge();
                }
            });
        }

        private void GetClanInvitationBadge()
        {
            CBSClan.GetUserInvitations(onGet => {
                if (onGet.IsSuccess)
                {
                    int invitesCount = onGet.Invites.Count;
                    UpdateCount(invitesCount);
                }
            });
        }

        private void GetClanUserRequests(string clanID)
        {
            CBSClan.GetClanUsersJoinRequests(clanID, onGet => {
                if (onGet.IsSuccess)
                {
                    int requestsCount = onGet.Profiles.Length;
                    UpdateCount(requestsCount);
                }
            });
        }

        // events
        private void OnUserJoinClan(AcceptClanInviteResult result)
        {
            CheckState();
        }

        private void OnUserLeaveClan(LeaveClanResult result)
        {
            CheckState();
        }

        private void OnUserCreateClan(CreateClanResult result)
        {
            CheckState();
        }

        private void OnUserRemoveClan(RemoveClanResult result)
        {
            CheckState();
        }

        private void OnUserAccepted(AcceptDeclineUserRequestResult result)
        {
            CheckState();
        }

        private void OnUserDeclined(AcceptDeclineUserRequestResult result)
        {
            CheckState();
        }

        private void OnUserDeclineClanInvation(DeclineInviteResult obj)
        {
            CheckState();
        }
    }
}

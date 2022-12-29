using CBS.Core;
using CBS.Scriptable;
using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class ClanInvationResult : MonoBehaviour, IScrollableItem<InvitationInfo>
    {
        [SerializeField]
        private Text Name;
        [SerializeField]
        private Text Expires;

        [SerializeField]
        private ClanAvatarDrawer Avatar;

        private string ClanID { get; set; }

        public void Display(InvitationInfo invitation)
        {
            ClanID = invitation.ClanID;
            Expires.text = invitation.Expires.ToLocalTime().ToString("MM/dd/yyyy H:mm");
            Avatar.LoadFromClanProfile(ClanID);
            Avatar.SetClickable(ClanID);

            CBSModule.Get<CBSClan>().GetClanInfo(ClanID, onGetInfo => {
                Name.text = onGetInfo.Info.GroupName;
            });
        }

        // button events
        public void OnAccept()
        {
            Debug.Log(ClanID);
            CBSModule.Get<CBSClan>().AcceptClanInvitatiion(ClanID, onAccept => {
                if (onAccept.IsSuccess)
                {
                    gameObject.SetActive(false);
                    new PopupViewer().ShowSimplePopup(new PopupRequest
                    {
                        Title = ClanTXTHandler.SuccessTitle,
                        Body = ClanTXTHandler.JoinClan,
                        OnOkAction = OnJoinClan
                    });
                }
                else
                {
                    new PopupViewer().ShowSimplePopup(new PopupRequest
                    {
                        Title = ClanTXTHandler.ErrorTitle,
                        Body = onAccept.Error.Stack
                    });
                }
            });
        }

        public void OnDecline()
        {
            CBSModule.Get<CBSClan>().DeclineClanInvite(ClanID, onDecline => { 
                if (onDecline.IsSuccess)
                {
                    gameObject.SetActive(false);
                }
            });
        }

        // events
        private void OnJoinClan()
        {
            var lobbyPrefabs = CBSScriptable.Get<ClanPrefabs>();

            var noClanPrefab = lobbyPrefabs.NoClanWindow;
            var clanPrefab = lobbyPrefabs.ClanWindow;

            var clanWindow = UIView.ShowWindow(clanPrefab);
            clanWindow.GetComponent<ClanWindow>().DisplayClan(ClanID);

            UIView.HideWindow(noClanPrefab);
        }
    }
}

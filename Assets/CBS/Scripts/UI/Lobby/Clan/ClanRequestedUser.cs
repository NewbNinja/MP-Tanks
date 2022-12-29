using CBS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class ClanRequestedUser : MonoBehaviour, IScrollableItem<ClanRequestUser>
    {
        [SerializeField]
        private Text DisplayName;
        [SerializeField]
        private Text Expires;
        [SerializeField]
        private GameObject AcceptBtn;
        [SerializeField]
        private GameObject DeclineBtn;
        [SerializeField]
        private AvatarDrawer Avatar;

        private string UserID { get; set; }
        private string EntityID { get; set; }
        private string ClanID { get; set; }
        private string AdminID { get; set; }

        private void OnDisable()
        {
            UserID = string.Empty;
            EntityID = string.Empty;
        }

        public void Display(ClanRequestUser profile)
        {
            UserID = profile.ProfileId;
            EntityID = profile.EntityId;
            ClanID = profile.ClanIdToJoin;
            AdminID = profile.ClanAdminID;

            var cbsProfile = CBSModule.Get<CBSProfile>();
            string viewerID = cbsProfile.PlayerID;
            Expires.text = profile.Expires.ToLocalTime().ToString("MM/dd/yyyy H:mm");
            // only admin can accept or decline request
            AcceptBtn.SetActive(AdminID == viewerID);
            DeclineBtn.SetActive(AdminID == viewerID);
            // draw avatar
            Avatar.LoadProfileAvatar(UserID);
            Avatar.SetClickable(UserID);

            CBSModule.Get<CBSProfile>().GetPlayerProfile(new CBSGetProfileRequest { 
                ProfileID = UserID
            }, OnProfileGetted);
        }

        private void OnProfileGetted(CBSGetProfileResult result)
        {
            if (result.IsSuccess)
                // draw nickname
                DisplayName.text = result.DisplayName;
        }

        // button events
        public void OnAccept()
        {
            CBSModule.Get<CBSClan>().AcceptUserJoinRequest(EntityID, ClanID, onAccept => {
                if (onAccept.IsSuccess)
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    new PopupViewer().ShowFabError(onAccept.Error);
                }
            });
        }

        public void OnDecline()
        {
            CBSModule.Get<CBSClan>().DeclineUserJoinRequest(EntityID, ClanID, onDecline => {
                if (onDecline.IsSuccess)
                {
                    gameObject.SetActive(false);
                }
            });
        }
    }
}

using CBS.Core;
using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class ClanUserUI : MonoBehaviour, IScrollableItem<ClanUser>
    {
        [SerializeField]
        private Text DisplayName;
        [SerializeField]
        private Text ClanRole;
        [SerializeField]
        private GameObject RemoveBtn;
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

        public void Display(ClanUser profile)
        {
            UserID = profile.ProfileId;
            EntityID = profile.EntityId;
            ClanID = profile.ClanId;
            AdminID = profile.ClanAdminID;

            // draw role
            ClanRole.text = profile.RoleName;

            var cbsProfile = CBSModule.Get<CBSProfile>();
            string viewerID = cbsProfile.PlayerID;
            // only admin can remove user
            RemoveBtn.SetActive(AdminID == viewerID);
            if (profile.IsAdmin)
            {
                RemoveBtn.SetActive(false);
            }

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
            {
                // draw nickname
                DisplayName.text = result.DisplayName;
            }
        }

        // button events
        public void OnRemove()
        {
            new PopupViewer().ShowYesNoPopup(new YesNoPopupRequest
            {
                Title = ClanTXTHandler.WarningTitle,
                Body = ClanTXTHandler.RemoveMemberWarning,
                OnYesAction = ProccessRemoveUser
            });
        }

        private void ProccessRemoveUser()
        {
            CBSModule.Get<CBSClan>().RemoveClanMember(EntityID, ClanID, onRemove => {
                if (onRemove.IsSuccess)
                {
                    gameObject.SetActive(false);
                }
            });
        }
    }
}

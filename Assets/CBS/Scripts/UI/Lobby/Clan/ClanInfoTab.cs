using CBS.Scriptable;
using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class ClanInfoTab : MonoBehaviour, ISetClan
    {
        [SerializeField]
        private Text DisplayName;
        [SerializeField]
        private Text Description;
        [SerializeField]
        private Text DateCreation;
        [SerializeField]
        private Text Members;
        [SerializeField]
        private GameObject RemoveBtn;
        [SerializeField]
        private GameObject LeaveBtn;
        [SerializeField]
        private ClanAvatarDrawer Avatar;

        public string ClanID { get; private set; }
        public string ClanName { get; private set; }

        private IClan CBSClan { get; set; }
        private IProfile Profile { get; set; }

        private ClanPrefabs Prefabs { get; set; }

        public void SetClanID(string clanID)
        {
            ClanID = clanID;
        }

        private void Awake()
        {
            CBSClan = CBSModule.Get<CBSClan>();
            Profile = CBSModule.Get<CBSProfile>();
            Prefabs = CBSScriptable.Get<ClanPrefabs>();
        }

        private void OnEnable()
        {
            RemoveBtn.SetActive(false);
            if (string.IsNullOrEmpty(ClanID))
                return;

            Avatar.LoadFromClanProfile(ClanID);

            CBSClan.GetClanInfo(ClanID, GetClanInfo);
        }

        private void GetClanInfo(GetClanInfoResult result)
        {
            if (result.IsSuccess)
            {
                var info = result.Info;
                ClanName = info.GroupName;

                DisplayName.text = info.GroupName;
                Description.text = info.Description;
                DateCreation.text = info.Created;
                Members.text = info.MembersCount.ToString();
                // only admin can remove clan
                string entityID = Profile.EntityID;
                RemoveBtn.SetActive(info.AdminID == entityID);
                // admin dont can leave clan
                LeaveBtn.SetActive(info.AdminID != entityID);
            }
        }

        // buttons events
        public void LeaveClan()
        {
            if (string.IsNullOrEmpty(ClanID))
                return;

            new PopupViewer().ShowYesNoPopup(new YesNoPopupRequest { 
                Title = ClanTXTHandler.WarningTitle,
                Body = ClanTXTHandler.LeaveClanWarning,
                OnYesAction = ProccessLeaveClan
            });
        }

        private void ProccessLeaveClan()
        {
            CBSClan.LeaveClan(ClanID, onRemove => {
                if (onRemove.IsSuccess)
                {
                    new PopupViewer().ShowSimplePopup(new PopupRequest
                    {
                        Title = ClanTXTHandler.SuccessTitle,
                        Body = ClanTXTHandler.LeaveClan,
                        OnOkAction = OnRemoveOrLeaveClan
                    });
                }
                else
                {
                    Debug.Log(onRemove.Error.Message);
                }
            });
        }

        public void RemoveClan()
        {
            if (string.IsNullOrEmpty(ClanID) || string.IsNullOrEmpty(ClanName))
                return;
            new PopupViewer().ShowYesNoPopup(new YesNoPopupRequest
            {
                Title = ClanTXTHandler.WarningTitle,
                Body = ClanTXTHandler.RemoveClanWarning,
                OnYesAction = ProccessRemoveClan
            });
        }

        private void ProccessRemoveClan()
        {
            CBSClan.RemoveClan(ClanID, ClanName, onRemove => {
                if (onRemove.IsSuccess)
                {
                    new PopupViewer().ShowSimplePopup(new PopupRequest
                    {
                        Title = ClanTXTHandler.SuccessTitle,
                        Body = ClanTXTHandler.RemoveClan,
                        OnOkAction = OnRemoveOrLeaveClan
                    });
                }
            });
        }

        private void OnRemoveOrLeaveClan()
        {
            var clanPrefab = Prefabs.ClanWindow;
            UIView.HideWindow(clanPrefab);
        }
    }
}

using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class ClanInfoForm : MonoBehaviour
    {
        [SerializeField]
        private Text DisplayName;
        [SerializeField]
        private Text Members;
        [SerializeField]
        private Text Date;
        [SerializeField]
        private Text Description;
        [SerializeField]
        private GameObject RequestButton;
        [SerializeField]
        private ClanAvatarDrawer Avatar;


        private IClan Clan { get; set; }
        private string ClanID { get; set; }

        private void Awake()
        {
            Clan = CBSModule.Get<CBSClan>();
        }

        public void Display(GetClanInfoResult result)
        {
            ClanID = result.Info.GroupId;

            DisplayName.text = result.Info.GroupName;
            Description.text = result.Info.Description;
            Date.text = result.Info.Created;
            Members.text = result.Info.MembersCount.ToString();

            RequestButton.SetActive(false);
            Clan.ExistInClan(OnCheckClanExist);

            Avatar.LoadFromClanProfile(ClanID);
        }

        private void OnCheckClanExist(ExistInClanResult result)
        {
            RequestButton.SetActive(!result.ExistInClan);
        }

        public void SendInviteRequest()
        {
            Clan.JoinClanRequest(ClanID, onJoin => {

                RequestButton.SetActive(!onJoin.IsSuccess);

                if (onJoin.IsSuccess)
                {
                    new PopupViewer().ShowSimplePopup(new PopupRequest
                    {
                        Title = ClanTXTHandler.SuccessTitle,
                        Body = ClanTXTHandler.ClanSendJoin
                    });
                }
                else
                {
                    new PopupViewer().ShowSimplePopup(new PopupRequest
                    {
                        Title = ClanTXTHandler.ErrorTitle,
                        Body = onJoin.Error.Message
                    });
                }
            });
        }
    }
}

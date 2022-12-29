using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class ClanSearchResult : MonoBehaviour
    {
        [SerializeField]
        private Text Name;
        [SerializeField]
        private ClanAvatarDrawer Avatar;

        private string ClanID { get; set; }

        public void Display(string clanID, string clanName)
        {
            ClanID = clanID;
            Name.text = clanName;
            Avatar.LoadFromClanProfile(clanID);
            Avatar.SetClickable(clanID);
        }

        public void JoinClan()
        {
            CBSModule.Get<CBSClan>().JoinClanRequest(ClanID, onJoin => {
                if (onJoin.IsSuccess)
                {
                    Debug.Log("Join success");
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

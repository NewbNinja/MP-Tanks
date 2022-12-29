using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class UserInfoForm : MonoBehaviour
    {
        [SerializeField]
        private Text DisplayName;
        [SerializeField]
        private Text Level;
        [SerializeField]
        private Text Exp;
        [SerializeField]
        private Text Clan;
        [SerializeField]
        private GameObject AddToFriendBtn;
        [SerializeField]
        private GameObject RemoveFriendBtn;
        [SerializeField]
        private GameObject DeclineFriendBtn;
        [SerializeField]
        private GameObject InviteToClanBtn;
        [SerializeField]
        private GameObject ViewClanBtn;
        [SerializeField]
        private AvatarDrawer AvatarDrawer;

        private CBSGetProfileResult CurrentInfo { get; set; }

        private string UserID { get; set; }
        private string EntityID { get; set; }

        private IFriends CBSFriends { get; set; }
        private IClan CBSClan { get; set; }
        private IProfile CBSProfile { get; set; }

        private void Awake()
        {
            CBSFriends = CBSModule.Get<CBSFriends>();
            CBSClan = CBSModule.Get<CBSClan>();
            CBSProfile = CBSModule.Get<CBSProfile>();
        }

        public void Display(CBSGetProfileResult info)
        {
            CurrentInfo = info;
            DisplayName.text = CurrentInfo.DisplayName;
            UserID = CurrentInfo.ProfileID;
            Level.text = CurrentInfo.LevelData.CurrentLevel.ToString() + " Level";
            Exp.text = CurrentInfo.LevelData.CurrentExp.ToString();
            EntityID = CurrentInfo.EntityID;


            var clanID = CurrentInfo.ClanData.ClanID;
            var clanName = CurrentInfo.ClanData.ClanName;
            var clanExists = !string.IsNullOrEmpty(clanID);
            ViewClanBtn.SetActive(clanExists);
            Clan.text = clanExists == true ? clanName : ClanTXTHandler.ClanNotExistTitle;
            // resize clan text
            var clanRect = Clan.GetComponent<RectTransform>();
            clanRect.sizeDelta = new Vector2(Clan.preferredWidth, clanRect.sizeDelta.y);
            // load avatar iamge
            var avatarUrl = info.AvatarURL;
            AvatarDrawer.LoadAvatarFromUrl(avatarUrl, UserID);

            CheckFriendsExist();
            CheckClanInvite();
        }

        private void CheckFriendsExist()
        {
            AddToFriendBtn.SetActive(false);
            RemoveFriendBtn.SetActive(false);
            DeclineFriendBtn.SetActive(false);

            CBSFriends.IsInFriends(UserID, onCheck => {
                if (onCheck.IsSuccess)
                {
                    AddToFriendBtn.SetActive(!onCheck.ExistAsAcceptedFriend && !onCheck.ExistAsRequestedFriend && !onCheck.WaitForUserAccept);
                    RemoveFriendBtn.SetActive(onCheck.ExistAsAcceptedFriend);
                    DeclineFriendBtn.SetActive(onCheck.ExistAsRequestedFriend);
                }
            });
        }

        private void CheckClanInvite()
        {
            InviteToClanBtn.SetActive(false);

            // get our clan info
            CBSClan.ExistInClan(onCheck => { 
                if (onCheck.ExistInClan)
                {
                    //Check a user for an active clan
                    CBSClan.ExistInClan(EntityID, onCheckUser => {
                        // If the user is not a member of the clan, continue
                        if (onCheckUser.IsSuccess && !onCheckUser.ExistInClan)
                        {
                            // only admin can invite to clan
                            string ourClanID = onCheck.ClanID;
                            string ourEntityID = CBSProfile.EntityID;
                            CBSClan.GetClanInfo(ourClanID, onGet => { 
                                if (onGet.IsSuccess)
                                {
                                    var clanInfo = onGet.Info;
                                    string adminID = clanInfo.AdminID;
                                    if (adminID == ourEntityID)
                                    {
                                        InviteToClanBtn.SetActive(true);
                                    }
                                }
                            });
                        }
                    });
                }
            });
        }

        // buttons events
        public void SendFriendsRequest()
        {
            CBSFriends.SendFriendsRequest(UserID, onSend => {
                if (onSend.IsSuccess)
                {
                    CheckFriendsExist();
                }
            });
        }

        public void RemoveFriend()
        {
            CBSFriends.RemoveFriend(UserID, onDecline => {
                if (onDecline.IsSuccess)
                {
                    CheckFriendsExist();
                }
            });
        }

        public void DeclineFriendRequest()
        {
            CBSFriends.DeclineFriendRequest(UserID, onDecline => {
                if (onDecline.IsSuccess)
                {
                    CheckFriendsExist();
                }
            });
        }

        public void SendDirectMessage()
        {
            var cbsChat = CBSModule.Get<CBSChat>();
            var chat = cbsChat.GetOrCreateChatWithUser(UserID);
            ChatUtils.ShowSimpleChat(chat);
        }

        public void InviteToClan()
        {
            CBSClan.ExistInClan(onCheck =>
            {
                if (onCheck.ExistInClan)
                {
                    string clanID = onCheck.ClanID;
                    CBSClan.InviteUser(clanID, EntityID, onInvite => { 
                        if (onInvite.IsSuccess)
                        {
                            InviteToClanBtn.SetActive(false);
                        }
                    });
                }
            });
        }

        public void ShowClanInfo()
        {
            var clanID = CurrentInfo.ClanData.ClanID;
            new PopupViewer().ShowClanInfo(clanID);
        }
    }
}

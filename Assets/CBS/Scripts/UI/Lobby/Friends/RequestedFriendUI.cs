using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class RequestedFriendUI : FriendUI
    {
        public void AcceptFriend()
        {
            string userID = CurrentFriend.ProfileID;
            Friends.AcceptFriend(userID, onAccept => { });
        }

        public void DeclineFriend()
        {
            string userID = CurrentFriend.ProfileID;
            Friends.DeclineFriendRequest(userID, onAccept => { });
        }
    }
}

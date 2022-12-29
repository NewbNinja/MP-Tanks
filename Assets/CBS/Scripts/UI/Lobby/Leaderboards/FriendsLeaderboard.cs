using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class FriendsLeaderboard : PlayerLeaderboard
    {
        protected override void GetPlayersLeaderboard()
        {
            Leaderboard.GetFriendsLeadearboard(OnGetLeaderboard);
        }
    }
}

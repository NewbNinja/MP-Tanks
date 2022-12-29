using CBS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class TournamentUser : LeaderboardUser, IScrollableItem<PlayerTournamnetEntry>
    {
        private PrizeObject PositionReward;

        public void Display(PlayerTournamnetEntry data)
        {
            base.Display(data);
            PositionReward = data.Reward;
        }

        // button click
        public void OnClickReward()
        {
            new PopupViewer().ShowRewardPopup(PositionReward);
        }
    }
}

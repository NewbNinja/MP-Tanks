using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class LeaderboardTab : MonoBehaviour
    {
        [SerializeField]
        private LeaderboardTabType TabType;

        public LeaderboardTabType GetTabType()
        {
            return TabType;
        }
    }

    public enum LeaderboardTabType
    {
        PLAYERS,
        CLANS,
        FRIENDS
    }
}

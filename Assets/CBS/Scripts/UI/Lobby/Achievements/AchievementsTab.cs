using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class AchievementsTab : MonoBehaviour
    {
        [SerializeField]
        private AchievementsTabType TabType;

        public AchievementsTabType GetTabType()
        {
            return TabType;
        }
    }

    public enum AchievementsTabType
    {
        ALL,
        ACTIVE,
        COMPLETED
    }
}

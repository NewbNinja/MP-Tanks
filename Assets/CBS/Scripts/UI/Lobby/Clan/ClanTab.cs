using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class ClanTab : MonoBehaviour
    {
        [SerializeField]
        private ClanTabType TabType;

        public ClanTabType GetTabType()
        {
            return TabType;
        }
    }

    public enum ClanTabType
    {
        GENERAL,
        INVITATIONS,
        SEARCH,
        MEMBERS,
        CHAT
    }
}

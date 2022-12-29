using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public interface ISetClan
    {
        string ClanID { get; }
        void SetClanID(string clanID);
    }
}

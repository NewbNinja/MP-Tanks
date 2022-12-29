using System;

namespace CBS
{
    [Serializable]
    public class ClanRequestUser
    {
        public string ClanAdminID;
        public string ClanIdToJoin;
        public string ProfileId;
        public string EntityId;
        public DateTime Expires;
    }
}

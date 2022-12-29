using System;

namespace CBS
{
    [Serializable]
    public class ClanUser
    {
        public string ClanAdminID;
        public string ClanId;
        public string ProfileId;
        public string EntityId;
        public string RoleId;
        public string RoleName;

        public bool IsAdmin => ProfileId == ClanAdminID;
    }
}

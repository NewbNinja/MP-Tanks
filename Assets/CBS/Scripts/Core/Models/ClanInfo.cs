using System;

namespace CBS
{
    [Serializable]
    public class ClanInfo
    {
        public string GroupId;
        public string GroupName;
        public string AdminID;
        public int MembersCount;
        public string MemberRoleId;
        public string AdminRoleId;
        public string Created;
        public string ImageURL;
        public string Description;
    }
}

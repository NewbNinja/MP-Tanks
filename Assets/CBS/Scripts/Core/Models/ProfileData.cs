using System;

namespace CBS
{
    [Serializable]
    public class ProfileData
    {
        public string ProfileID;
        public string DisplayName;
        public string AvatarUrl;
        public object LevelData;
        public object ClanData;
        public string EntityID;
    }
}

using System;
using System.Collections.Generic;

namespace CBS
{
    [Serializable]
    public class GetProfileCallback
    {
        public PlayerLeaderboardEntry ProfileResult;
        public List<PlayerLeaderboardEntry> Leaderboards;
    }

    [Serializable]
    public class GetClanProfileCallback
    {
        public ClanLeaderboardEntry ProfileResult;
        public List<ClanLeaderboardEntry> Leaderboards;
    }


    [Serializable]
    public class PlayerLeaderboardEntry
    {
        public string PlayFabId;
        public string DisplayName;
        public int StatValue;
        public int Position;
        public string AvatarUrl;
    }

    [Serializable]
    public class ClanLeaderboardEntry
    {
        public string ClanId;
        public string ClanName;
        public int StatValue;
        public int Position;
        public string CurrentClanId;
    }
}
using System;
using System.Collections.Generic;

namespace CBS
{
    [Serializable]
    public class TournamentFinishCallback
    {
        public string ProfileID;
        public int Position;
        public int PositionValue;
        public PrizeObject Prize;

        public string FinishTournamentID;
        public string FinishTournamentName;

        public TournamentMigration NewMigration;
        public string NewTournamentID;
        public string NewTournamentName;
    }

    [Serializable]
    public class TournamentStateCallback
    {
        public string ProfileID;
        public string PlayerTournamentID;
        public string TournamentName;
        public bool Joined;
        public bool Finished;
        public int TimeLeft;
        public List<PlayerTournamnetEntry> Leaderboard;
    }

    [Serializable]
    public class TournamentObject
    {
        public string TounamentID;
        public string TournamentName;
        public bool IsDefault;
        public string NextTournamentID;
        public string DowngradeTournamentID;

        public List<TournamentPosition> Positions;
    }

    [Serializable]
    public class TournamentData
    {
        public TournamentDate Date;
        public int DateTimestamp;
        public Dictionary<string, TournamentObject> Tournaments;
    }

    [Serializable]
    public class TournamentPosition
    {
        public bool NextTournament;
        public bool DowngradeTournament;
        public PrizeObject Prizes;
    }

    [Serializable]
    public class PlayerTournamnetEntry : PlayerLeaderboardEntry
    {
        public PrizeObject Reward;
    }

    public enum TournamentDate
    {
        Hourly,
        Daily,
        Weekly,
        Monthly,
        Yearly,
        Custom
    }

    public enum TournamentMigration
    {
        NONE = 0,
        UP = 1,
        DOWN = 2
    }
}

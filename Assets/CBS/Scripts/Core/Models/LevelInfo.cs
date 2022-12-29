using System;

namespace CBS
{
    [Serializable]
    public class LevelInfo
    {
        public int CurrentLevel;
        public int PrevLevelExp;
        public int NextLevelExp;
        public int CurrentExp;
        public bool NewLevelReached;
        public PrizeObject NewLevelPrize;

        public bool ReachMaxLevel
        {
            get
            {
                return CurrentExp >= NextLevelExp && CurrentExp != 0;
            }
        }
    }
}

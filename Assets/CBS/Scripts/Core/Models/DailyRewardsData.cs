using System;
using System.Collections.Generic;

namespace CBS
{
    [Serializable]
    public class DailyBonusResultData
    {
        public int CurrentDailyIndex;
        public bool Picked;
        public DailyBonusTable DailyData;
    }

    [Serializable]
    public class DailyBonusTable
    {
        public List<PrizeObject> DaliyPrizes;
    }

    [Serializable]
    public class DailyBonusInfo
    {
        public int DayNumber;
        public bool IsCurrent;
        public bool IsPicked;
        public PrizeObject Prize;
    }
}

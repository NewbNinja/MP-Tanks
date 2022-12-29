using System;
using System.Collections.Generic;

namespace CBS
{
    [Serializable]
    public class RouletteTable
    {
        public List<RoulettePosition> Positions;
    }

    [Serializable]
    public class RoulettePosition
    {
        public string ID;
        public string DisplayName;
        public int Weight;
        public PrizeObject Prize;
    }
}

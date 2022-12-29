using System;
using System.Collections.Generic;

namespace CBS
{
    [Serializable]
    public class LevelTable
    {
        public PrizeObject RegistrationPrize;
        public List<LevelDetail> Table = new List<LevelDetail>();
    }
}

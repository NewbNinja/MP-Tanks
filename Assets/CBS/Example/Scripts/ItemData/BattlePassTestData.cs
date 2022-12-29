using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Example
{
    public class BattlePassTestData : CBSBattlePassCustomData
    {
        public string SomeString;
        public int SomeIntData;
        public float SomeFloatData;
        public BattlePassType Type;
    }

    public enum BattlePassType {
        SIMPLE,
        PREMIUM
    }
}

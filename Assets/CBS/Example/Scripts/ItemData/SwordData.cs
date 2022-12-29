using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Example
{
    public class SwordData : CBSItemData
    {
        public string Command;
        public int Attack;
        public float Speed;
        public int Bonus;
        public bool IsHeroic;
        public SwordHand Hand;
    }

    public enum SwordHand
    {
        ONE_HAND_WEPOAN,
        TWO_HAND_WEAPON
    }

}

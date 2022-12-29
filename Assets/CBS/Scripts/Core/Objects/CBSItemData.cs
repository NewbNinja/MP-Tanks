using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace CBS
{
    [Serializable]
    public class CBSItemData : CBSBaseCustomData
    {
        public bool IsEquippable;
        public bool IsConsumable;
        public bool IsTradable;
        public ItemType ItemType;
    }
}

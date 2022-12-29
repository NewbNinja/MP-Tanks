using CBS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class ShopTab : MonoBehaviour
    {
        [SerializeField]
        private ItemType Tab;

        public ItemType GetTab()
        {
            return Tab;
        }
    }
}

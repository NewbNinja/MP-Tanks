using System;
using System.Collections.Generic;

namespace CBS
{
    [Serializable]
    public class PrizeObject
    {
        public List<string> BundledItems;

        public List<string> Lootboxes;

        public Dictionary<string, uint> BundledVirtualCurrencies;

        public bool IsEmpty()
        {
            return (BundledItems == null || BundledItems.Count == 0)
                && (Lootboxes == null || Lootboxes.Count == 0)
                && (BundledVirtualCurrencies == null && BundledVirtualCurrencies.Count == 0);

        }

        public int GetPositionCount()
        {
            var bundleCount = BundledItems == null ? 0 : BundledItems.Count;
            var lootCount = Lootboxes == null ? 0 : Lootboxes.Count;
            var currencyCount = BundledVirtualCurrencies == null ? 0 : BundledVirtualCurrencies.Count;
            return bundleCount + lootCount + currencyCount;
        }
    }
}
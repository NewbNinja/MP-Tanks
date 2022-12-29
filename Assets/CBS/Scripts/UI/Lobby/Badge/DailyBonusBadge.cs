using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class DailyBonusBadge : BaseBadge
    {
        private IDailyBonus DailyBonus { get; set; }

        private void Awake()
        {
            DailyBonus = CBSModule.Get<CBSDailyBonus>();
        }

        private void OnEnable()
        {
            DailyBonus.OnRewardCollected += OnRewardCollected;
            UpdateCount(0);
            GetDailyBonus();
        }

        private void OnDisable()
        {
            DailyBonus.OnRewardCollected += OnRewardCollected;
        }

        private void GetDailyBonus()
        {
            DailyBonus.GetDailyBonus(OnGetDailyBonus);
        }

        // events
        private void OnRewardCollected(CollectDailyBonusResult result)
        {
            GetDailyBonus();
        }

        private void OnGetDailyBonus(GetDailyBonusResult result)
        {
            if (result.IsSuccess)
            {
                var badgeCount = result.Picked ? 0 : 1;
                UpdateCount(badgeCount);
            }
        }
    }
}

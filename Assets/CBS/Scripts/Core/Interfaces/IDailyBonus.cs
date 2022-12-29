using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS
{
    public interface IDailyBonus
    {
        /// <summary>
        /// Notifies when a user has received a reward
        /// </summary>
        event Action<CollectDailyBonusResult> OnRewardCollected;

        /// <summary>
        /// Get information about the status of the current user's daily rewards. Also get a list of all daily rewards.
        /// </summary>
        /// <param name="result"></param>
        void GetDailyBonus(Action<GetDailyBonusResult> result);

        /// <summary>
        /// Collect the daily reward.
        /// </summary>
        /// <param name="result"></param>
        void CollectDailyBonus(Action<CollectDailyBonusResult> result);

        /// <summary>
        /// Reset daily bonus reward state of current user.
        /// </summary>
        /// <param name="result"></param>
        void ResetDailyBonus(Action<ResetDailyBonusResult> result);
    }
}

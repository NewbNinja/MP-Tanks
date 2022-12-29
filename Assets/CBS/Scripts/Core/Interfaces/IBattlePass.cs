using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS
{
    public interface IBattlePass
    {
        /// <summary>
        /// Notify when the reward was received.
        /// </summary>
        event Action<PrizeObject> OnRewardRecived;
        /// <summary>
        /// Notify when experience has been gained for a specific Battle Pass.
        /// </summary>
        event Action<string, int> OnExpirienceAdded;
        /// <summary>
        /// Notify when premium access has been unlocked for a specific Battle Pass.
        /// </summary>
        event Action<string> OnPremiumAccessGranted;
        /// <summary>
        /// Get information about the state of the player's instances of Battle passes. Does not contain complete information about levels and rewards. More suitable for implementing badges.
        /// </summary>
        /// <param name="result"></param>
        void GetPlayerStates(Action<GetPlayerBattlePassStatesResult> result);
        /// <summary>
        /// Get information about the state of the player's instances of Battle passes. Does not contain complete information about levels and rewards. More suitable for implementing badges
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        void GetPlayerStates(BattlePassPlayerStateRequest request, Action<GetPlayerBattlePassStatesResult> result);
        /// <summary>
        /// Get complete information about the state of the player's instances of Battle passes and instance levels. Contains complete information about levels and rewards.
        /// </summary>
        /// <param name="battlePassID"></param>
        /// <param name="result"></param>
        void GetBattlePassFullInformation(string battlePassID, Action<GetBattlePassFullInformationResult> result);
        /// <summary>
        /// Add player experience for a specific instance of Battle Pass.
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="exp"></param>
        /// <param name="result"></param>
        void AddExpirienceToInstance(string instanceID, int exp, Action<AddExpirienceToInstanceResult> result);
        /// <summary>
        /// Add player experience for all active instances of Battle Passes.
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="result"></param>
        void AddExpirienceToAllActiveInstances(int exp, Action<AddExpirienceToAllInstancesResult> result);
        /// <summary>
        /// Grant the player a reward from a specific instance of Battle Pass.
        /// </summary>
        /// <param name="battlePassID"></param>
        /// <param name="level"></param>
        /// <param name="isPremium"></param>
        /// <param name="result"></param>
        void GrantAwardToPlayer(string battlePassID, int level, bool isPremium, Action<GrandAwardToPlayerResult> result);
        /// <summary>
        /// Give the player premium access for a specific instance of Battle Pass.
        /// </summary>
        /// <param name="battlePassID"></param>
        /// <param name="result"></param>
        void GrantPremiumAccessToPlayer(string battlePassID, Action<GrantPremiumAccessResult> result);
        /// <summary>
        /// Reset player data for a specific Battle Pass.
        /// </summary>
        /// <param name="battlePassID"></param>
        /// <param name="result"></param>
        void ResetBattlePassPlayerState(string battlePassID, Action<ResetBattlePassStateResult> result);
    }
}

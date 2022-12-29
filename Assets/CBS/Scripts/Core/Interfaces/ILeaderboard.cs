using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS
{
    public interface ILeaderboard
    {
        /// <summary>
        /// Get users leaderboard based by player level/xp
        /// </summary>
        /// <param name="result"></param>
        void GetLevelLeadearboard(Action<GetLeaderboardResult> result);

        /// <summary>
        /// Get friends leaderboard based by player level/xp
        /// </summary>
        /// <param name="result"></param>
        void GetFriendsLeadearboard(Action<GetLeaderboardResult> result);

        /// <summary>
        /// Get users leaderboard based on custom statistics.
        /// </summary>
        /// <param name="statisticName"></param>
        /// <param name="result"></param>
        void GetLeadearboardByStatistic(string statisticName, Action<GetLeaderboardResult> result);

        /// <summary>
        /// Update custom statisitc points
        /// </summary>
        /// <param name="statisticName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        void UpdateStatisticPoint(string statisticName, int value, Action<UpdateStatisticResult> result);

        /// <summary>
        /// Add custom statisitc points
        /// </summary>
        /// <param name="statisticName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        void AddStatisticPoint(string statisticName, int value, Action<UpdateStatisticResult> result);

        /// <summary>
        /// Add clan statistic point
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        void AddClanStatisticPoint(string clanID, int value, Action<UpdateStatisticResult> result);

        /// <summary>
        /// Update clan statistic point
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        void UpdateClanStatisticPoint(string clanID, int value, Action<UpdateStatisticResult> result);

        /// <summary>
        /// Get clans leaderboard
        /// </summary>
        /// <param name="result"></param>
        void GetClansLeaderboard(Action<GetClanLeaderboardResult> result);

        /// <summary>
        /// Reset all players statistics, include all leaderboards and player exp/level.
        /// </summary>
        /// <param name="result"></param>
        void ResetAllPlayersStatistics(Action<ResetPlayersStatisticsResult> result);

        /// <summary>
        /// Get leaderboard around user based by player level/xp
        /// </summary>
        /// <param name="result"></param>
        void GetLevelLeadearboardAroundPlayer(Action<GetLeaderboardResult> result);

        /// <summary>
        /// Get leaderboard around user based on custom statistics.
        /// </summary>
        /// <param name="statisticName"></param>
        /// <param name="result"></param>
        void GetLeadearboardAroundUserByStatistic(string statisticName, Action<GetLeaderboardResult> result);
    }
}

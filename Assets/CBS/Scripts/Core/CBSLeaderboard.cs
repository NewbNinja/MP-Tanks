using CBS.Playfab;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS
{
    public class CBSLeaderboard : CBSModule, ILeaderboard
    {
        private IFabLeaderboard FabLeaderboard { get; set; }
        private IProfile Profile { get; set; }

        protected override void Init()
        {
            FabLeaderboard = FabExecuter.Get<FabLeaderboard>();
            Profile = Get<CBSProfile>();
        }

        /// <summary>
        /// Get users leaderboard based by player level/xp
        /// </summary>
        /// <param name="result"></param>
        public void GetLevelLeadearboard(Action<GetLeaderboardResult> result)
        {
            string statisticName = CBSConstants.StaticsticExpKey;
            GetLeadearboardByStatistic(statisticName, result);
        }

        /// <summary>
        /// Get leaderboard around user based by player level/xp
        /// </summary>
        /// <param name="result"></param>
        public void GetLevelLeadearboardAroundPlayer(Action<GetLeaderboardResult> result)
        {
            string statisticName = CBSConstants.StaticsticExpKey;
            GetLeadearboardAroundUserByStatistic(statisticName, result);
        }

        /// <summary>
        /// Get users leaderboard based on custom statistics.
        /// </summary>
        /// <param name="statisticName"></param>
        /// <param name="result"></param>
        public void GetLeadearboardByStatistic(string statisticName, Action<GetLeaderboardResult> result)
        {
            string profileID = Profile.PlayerID;

            FabLeaderboard.GetLeaderboard(new GetLeaderboardRequest
            {
                ProfileID = profileID,
                StatisticName = statisticName,
                MaxCount = CBSConstants.MaxLeaderboardCount
            }, onGet => {
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetLeaderboardResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawData = onGet.FunctionResult.ToString();
                    var leaderboardResult = JsonUtility.FromJson<GetProfileCallback>(rawData);
                    result?.Invoke(new GetLeaderboardResult
                    {
                        IsSuccess = true,
                        ProfileResult = leaderboardResult.ProfileResult,
                        Leaderboards = leaderboardResult.Leaderboards
                    });
                }
            }, onFailed => {
                result?.Invoke(new GetLeaderboardResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Get leaderboard around user based on custom statistics.
        /// </summary>
        /// <param name="statisticName"></param>
        /// <param name="result"></param>
        public void GetLeadearboardAroundUserByStatistic(string statisticName, Action<GetLeaderboardResult> result)
        {
            string profileID = Profile.PlayerID;

            FabLeaderboard.GetLeaderboardAroundPlayer(new GetLeaderboardRequest
            {
                ProfileID = profileID,
                StatisticName = statisticName,
                MaxCount = CBSConstants.MaxLeaderboardCount
            }, onGet => {
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetLeaderboardResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawData = onGet.FunctionResult.ToString();
                    var leaderboardResult = JsonUtility.FromJson<GetProfileCallback>(rawData);
                    result?.Invoke(new GetLeaderboardResult
                    {
                        IsSuccess = true,
                        ProfileResult = leaderboardResult.ProfileResult,
                        Leaderboards = leaderboardResult.Leaderboards
                    });
                }
            }, onFailed => {
                result?.Invoke(new GetLeaderboardResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Get clans leaderboard
        /// </summary>
        /// <param name="result"></param>
        public void GetClansLeaderboard(Action<GetClanLeaderboardResult> result)
        {
            string viewerEntityID = Profile.EntityID;

            FabLeaderboard.GetLeaderboardOfClanAdmins(new GetClanLeaderboardRequest
            {
                ViewerEntityID = viewerEntityID,
                MaxCount = CBSConstants.MaxClanLeaderboardCount,
                StatisticName = CBSConstants.ClanStatisticKey
            }, onGet => { 
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetClanLeaderboardResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawData = onGet.FunctionResult.ToString();
                    var leaderboardResult = JsonUtility.FromJson<GetClanProfileCallback>(rawData);
                    result?.Invoke(new GetClanLeaderboardResult { 
                        IsSuccess = true,
                        ProfileResult = leaderboardResult.ProfileResult,
                        Leaderboards = leaderboardResult.Leaderboards
                    });
                }
            }, onFailed => {
                result?.Invoke(new GetClanLeaderboardResult
                { 
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Get friends leaderboard based by player level/xp
        /// </summary>
        /// <param name="result"></param>
        public void GetFriendsLeadearboard(Action<GetLeaderboardResult> result)
        {
            string profileID = Profile.PlayerID;

            FabLeaderboard.GetFriendsLeaderboard(new GetLeaderboardRequest
            {
                ProfileID = profileID,
                StatisticName = CBSConstants.StaticsticExpKey,
                MaxCount = CBSConstants.MaxLeaderboardCount
            }, onGet => {
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetLeaderboardResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawData = onGet.FunctionResult.ToString();
                    var leaderboardResult = JsonUtility.FromJson<GetProfileCallback>(rawData);
                    result?.Invoke(new GetLeaderboardResult
                    {
                        IsSuccess = true,
                        ProfileResult = leaderboardResult.ProfileResult,
                        Leaderboards = leaderboardResult.Leaderboards
                    });
                }
            }, onFailed => {
                result?.Invoke(new GetLeaderboardResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Update custom statisitc points
        /// </summary>
        /// <param name="statisticName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        public void UpdateStatisticPoint(string statisticName, int value, Action<UpdateStatisticResult> result)
        {
            string profileID = Profile.PlayerID;

            FabLeaderboard.UpdateStatisticPoint(new UpdateStatisticRequest {
                ProfileID = profileID,
                StatisticName = statisticName,
                Value = value
            }, onUpdate => {
                if (onUpdate.Error != null)
                {
                    result?.Invoke(new UpdateStatisticResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onUpdate.Error)
                    });
                }
                else
                {
                    result?.Invoke(new UpdateStatisticResult
                    {
                        IsSuccess = true
                    });
                }
            }, onFailed => {
                result?.Invoke(new UpdateStatisticResult {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Add custom statisitc points
        /// </summary>
        /// <param name="statisticName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        public void AddStatisticPoint(string statisticName, int value, Action<UpdateStatisticResult> result)
        {
            string profileID = Profile.PlayerID;

            FabLeaderboard.AddStatisticPoint(new UpdateStatisticRequest
            {
                ProfileID = profileID,
                StatisticName = statisticName,
                Value = value
            }, onUpdate => {
                if (onUpdate.Error != null)
                {
                    result?.Invoke(new UpdateStatisticResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onUpdate.Error)
                    });
                }
                else
                {
                    result?.Invoke(new UpdateStatisticResult
                    {
                        IsSuccess = true
                    });
                }
            }, onFailed => {
                result?.Invoke(new UpdateStatisticResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Add clan statistic point
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        public void AddClanStatisticPoint(string clanID, int value, Action<UpdateStatisticResult> result)
        {
            FabLeaderboard.AddClanStatisticPoint(new UpdateClanStatisticRequest
            {
                ClanID = clanID,
                StatisticName = CBSConstants.ClanStatisticKey,
                Value = value
            }, onUpdate => {
                if (onUpdate.Error != null)
                {
                    result?.Invoke(new UpdateStatisticResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onUpdate.Error)
                    });
                }
                else
                {
                    result?.Invoke(new UpdateStatisticResult
                    {
                        IsSuccess = true
                    });
                }
            }, onFailed => {
                result?.Invoke(new UpdateStatisticResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Update clan statistic point
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        public void UpdateClanStatisticPoint(string clanID, int value, Action<UpdateStatisticResult> result)
        {
            FabLeaderboard.UpdateClanStatisticPoint(new UpdateClanStatisticRequest
            {
                ClanID = clanID,
                StatisticName = CBSConstants.ClanStatisticKey,
                Value = value
            }, onUpdate => {
                if (onUpdate.Error != null)
                {
                    result?.Invoke(new UpdateStatisticResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onUpdate.Error)
                    });
                }
                else
                {
                    result?.Invoke(new UpdateStatisticResult
                    {
                        IsSuccess = true
                    });
                }
            }, onFailed => {
                result?.Invoke(new UpdateStatisticResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Reset all players statistics, include all leaderboards and player exp/level.
        /// </summary>
        /// <param name="result"></param>
        public void ResetAllPlayersStatistics(Action<ResetPlayersStatisticsResult> result)
        {
            string profileID = Profile.PlayerID;

            FabLeaderboard.ResetPlayerLeaderboards(profileID, onReset => {
                if (onReset.Error != null)
                {
                    result?.Invoke(new ResetPlayersStatisticsResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onReset.Error)
                    });
                }
                else
                {
                    result?.Invoke(new ResetPlayersStatisticsResult
                    {
                        IsSuccess = true
                    });
                }
            }, onFailed => {
                result?.Invoke(new ResetPlayersStatisticsResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }
    }

    public struct GetLeaderboardResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public PlayerLeaderboardEntry ProfileResult;
        public List<PlayerLeaderboardEntry> Leaderboards;
    }

    public struct GetClanLeaderboardResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public ClanLeaderboardEntry ProfileResult;
        public List<ClanLeaderboardEntry> Leaderboards;
    }

    public struct UpdateStatisticResult
    {
        public bool IsSuccess;
        public SimpleError Error;
    }

    public struct ResetPlayersStatisticsResult
    {
        public bool IsSuccess;
        public SimpleError Error;
    }
}
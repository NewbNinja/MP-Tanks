using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CBS.Playfab;
using PlayFab;
using System.Linq;

namespace CBS
{
    public class CBSTournament : CBSModule, ITournament
    {
        private IFabTournament FabTournament { get; set; }
        private IProfile Profile { get; set; }

        protected override void Init()
        {
            FabTournament = FabExecuter.Get<FabTournament>();
            Profile = Get<CBSProfile>();
        }

        /// <summary>
        /// Get complete information about the current state of the user in the tournament
        /// </summary>
        /// <param name="result"></param>
        public void GetPlayerCurrentTournamentState(Action<GetTournamentStateResult> result)
        {
            string profileID = Profile.PlayerID;
            FabTournament.GetTournamentState(profileID, onGet => {
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetTournamentStateResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    string rawData = onGet.FunctionResult.ToString();
                    var callbackData = jsonPlugin.DeserializeObject<TournamentStateCallback>(rawData);
                    result?.Invoke(new GetTournamentStateResult
                    {
                        IsSuccess = true,
                        ProfileID = callbackData.ProfileID,
                        Joined = callbackData.Joined,
                        PlayerTournamentID = callbackData.PlayerTournamentID,
                        TournamentName = callbackData.TournamentName,
                        Leaderboard = callbackData.Leaderboard,
                        Finished = callbackData.Finished,
                        TimeLeft = callbackData.TimeLeft
                    });
                }
            }, onFailed => {
                result?.Invoke(new GetTournamentStateResult { 
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Enter the tournament based on the previous achievements in the user's tournaments.
        /// </summary>
        /// <param name="result"></param>
        public void FindAndJoinTournament(Action<JoinTournamentResult> result)
        {
            string profileID = Profile.PlayerID;
            string entityID = Profile.EntityID;

            FabTournament.FindAndJoinTournament(profileID, entityID, onJoin => { 
                if (onJoin.Error != null)
                {
                    result?.Invoke(new JoinTournamentResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onJoin.Error)
                    });
                }
                else
                {
                    string groupId = onJoin.FunctionResult.ToString();
                    result?.Invoke(new JoinTournamentResult
                    {
                        IsSuccess = true,
                        JoinedTournamentID = groupId
                    });
                }
            }, onFailed => {
                result?.Invoke(new JoinTournamentResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Leave the current tournament.
        /// </summary>
        /// <param name="result"></param>
        public void LeaveCurrentTournament(Action<LeaveTournamentResult> result)
        {
            string profileID = Profile.PlayerID;
            string entityID = Profile.EntityID;

            FabTournament.LeaveTournament(profileID, entityID, onLeave => { 
                if (onLeave.Error != null)
                {
                    result?.Invoke(new LeaveTournamentResult
                    {
                        Error = SimpleError.FromTemplate(onLeave.Error),
                        IsSuccess = false
                    });
                }
                else
                {
                    result?.Invoke(new LeaveTournamentResult
                    {
                        IsSuccess = true
                    });
                }
            }, onFailed => {
                result?.Invoke(new LeaveTournamentResult { 
                    Error = SimpleError.FromTemplate(onFailed),
                    IsSuccess = false
                });
            });
        }

        /// <summary>
        /// Add points to the player in the tournament.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="result"></param>
        public void AddTournamentPoint(int point, Action<UpdateTournamentPointResult> result)
        {
            string profileID = Profile.PlayerID;

            FabTournament.AddTournamentPoint(profileID, point, onAdd => { 
                if (onAdd.Error != null)
                {
                    result?.Invoke(new UpdateTournamentPointResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onAdd.Error)
                    });
                }
                else
                {
                    result?.Invoke(new UpdateTournamentPointResult
                    {
                        IsSuccess = true
                    });
                }
            }, onFailed => {
                result?.Invoke(new UpdateTournamentPointResult { 
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Add players points in the tournament.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="result"></param>
        public void UpdateTournamentPoint(int point, Action<UpdateTournamentPointResult> result)
        {
            string profileID = Profile.PlayerID;

            FabTournament.UpdateTournamentPoint(profileID, point, onAdd => {
                if (onAdd.Error != null)
                {
                    result?.Invoke(new UpdateTournamentPointResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onAdd.Error)
                    });
                }
                else
                {
                    result?.Invoke(new UpdateTournamentPointResult
                    {
                        IsSuccess = true
                    });
                }
            }, onFailed => {
                result?.Invoke(new UpdateTournamentPointResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Finish the tournament and get a reward.
        /// </summary>
        /// <param name="result"></param>
        public void FinishTournament(Action<FinishTournamentResult> result)
        {
            string profileID = Profile.PlayerID;
            string entityID = Profile.EntityID;

            FabTournament.FinishTournament(profileID, entityID, onFinish => { 
                if (onFinish.Error != null)
                {
                    result?.Invoke(new FinishTournamentResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onFinish.Error)
                    });
                }
                else
                {
                    Debug.Log(onFinish.FunctionResult);
                    var rawData = onFinish.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var resultObject = jsonPlugin.DeserializeObject<TournamentFinishCallback>(rawData);

                    var prizes = resultObject.Prize;
                    if (prizes != null)
                    {
                        var currencies = prizes.BundledVirtualCurrencies;
                        if (currencies != null)
                        {
                            var codes = currencies.Select(x => x.Key).ToArray();
                            Get<CBSCurrency>().ChangeRequest(codes);
                        }
                    }

                    result?.Invoke(new FinishTournamentResult
                    {
                        IsSuccess = true,
                        ProfileID = resultObject.ProfileID,
                        Position = resultObject.Position,
                        Prize = resultObject.Prize,
                        NewTournamentID = resultObject.NewTournamentID,
                        NewTournamentName = resultObject.NewTournamentName
                    });
                }
            }, onFailed => {
                result?.Invoke(new FinishTournamentResult { 
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Get tournament information by specific id.
        /// </summary>
        /// <param name="tournamentID"></param>
        /// <param name="result"></param>
        public void GetTournamentByID(string tournamentID, Action<GetTournamentDataResult> result)
        {
            FabTournament.GetTournamentDataByID(tournamentID, onGet => {
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetTournamentDataResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawData = onGet.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var tournament = jsonPlugin.DeserializeObject<TournamentObject>(rawData);

                    result?.Invoke(new GetTournamentDataResult {
                        IsSuccess = true,
                        Tournament = tournament
                    });
                }
            }, onFailed => {
                result?.Invoke(new GetTournamentDataResult {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Get information of all tournaments.
        /// </summary>
        /// <param name="result"></param>
        public void GetAllTournaments(Action<GetAllTournamentsResult> result)
        {
            FabTournament.GetAllTournaments(onGet => {
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetAllTournamentsResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawData = onGet.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var tournaments = jsonPlugin.DeserializeObject<TournamentData>(rawData);

                    result?.Invoke(new GetAllTournamentsResult
                    {
                        IsSuccess = true,
                        Tournaments = tournaments
                    });
                }
            }, onFailed => {
                result?.Invoke(new GetAllTournamentsResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }
    }

    public struct GetTournamentStateResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string ProfileID;
        public string PlayerTournamentID;
        public string TournamentName;
        public bool Joined;
        public bool Finished;
        public int TimeLeft;
        public List<PlayerTournamnetEntry> Leaderboard;
    }

    public struct GetTournamentDataResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public TournamentObject Tournament;
    }

    public struct GetAllTournamentsResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public TournamentData Tournaments;
    }

    public struct JoinTournamentResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string JoinedTournamentID;
    }

    public struct LeaveTournamentResult
    {
        public bool IsSuccess;
        public SimpleError Error;
    }

    public struct UpdateTournamentPointResult
    {
        public bool IsSuccess;
        public SimpleError Error;
    }

    public struct FinishTournamentResult
    {
        public bool IsSuccess;
        public SimpleError Error;

        public string ProfileID;
        public int Position;
        public PrizeObject Prize;
        public string NewTournamentID;
        public string NewTournamentName;
    }
}

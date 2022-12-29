using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS
{
    public interface ITournament
    {
        /// <summary>
        /// Get complete information about the current state of the user in the tournament
        /// </summary>
        /// <param name="result"></param>
        void GetPlayerCurrentTournamentState(Action<GetTournamentStateResult> result);

        /// <summary>
        /// Enter the tournament based on the previous achievements in the user's tournaments.
        /// </summary>
        /// <param name="result"></param>
        void FindAndJoinTournament(Action<JoinTournamentResult> result);

        /// <summary>
        /// Leave the current tournament.
        /// </summary>
        /// <param name="result"></param>
        void LeaveCurrentTournament(Action<LeaveTournamentResult> result);

        /// <summary>
        /// Add points to the player in the tournament.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="result"></param>
        void AddTournamentPoint(int point, Action<UpdateTournamentPointResult> result);

        /// <summary>
        /// Add players points in the tournament.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="result"></param>
        void UpdateTournamentPoint(int point, Action<UpdateTournamentPointResult> result);

        /// <summary>
        /// Finish the tournament and get a reward.
        /// </summary>
        /// <param name="result"></param>
        void FinishTournament(Action<FinishTournamentResult> result);

        /// <summary>
        /// Get tournament information by specific id.
        /// </summary>
        /// <param name="tournamentID"></param>
        /// <param name="result"></param>
        void GetTournamentByID(string tournamentID, Action<GetTournamentDataResult> result);

        /// <summary>
        /// Get information of all tournaments.
        /// </summary>
        /// <param name="result"></param>
        void GetAllTournaments(Action<GetAllTournamentsResult> result);
    }
}

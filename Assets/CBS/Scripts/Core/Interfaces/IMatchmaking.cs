using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS
{
    public interface IMatchmaking
    {
        /// <summary>
        /// Notifies about status change in Matchmaking
        /// </summary>
        event Action<MatchmakingStatus> OnStatusChanged;

        /// <summary>
        /// Notifies about the successful completion of the search for opponents.
        /// </summary>
        event Action<StartMatchResult> OnMatchStart;

        /// <summary>
        /// Current Queue name
        /// </summary>
        string ActiveQueue { get; }

        /// <summary>
        /// Current ticket id name
        /// </summary>
        string ActiveTicketID { get; }

        /// <summary>
        /// Active matchmaking status
        /// </summary>
        MatchmakingStatus Status { get; }

        /// <summary>
        /// Get a list of all Queues on the server
        /// </summary>
        /// <param name="result"></param>
        void GetMatchmakingQueuesList(Action<GetMatchmakingListResult> result);

        /// <summary>
        /// Creates a ticket to find opponents. After a successful call, listen for a change in the status of the queue.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        /// <param name="latencies"></param>
        void FindMatch(FindMatchRequest request, Action<FindMatchResult> result, Latency[] latencies=null);

        
        
        
        
        /// <summary>
        /// Get a detailed description of the queue by name.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="result"></param>
        void GetMatchmakingQueue(string queueName, Action<GetQueueResult> result);

        /// <summary>
        /// Cancels all search tickets for the current player.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="result"></param>
        void CancelMatch(string queueName, Action<CancelMatchResult> result);

        
        /// <summary>
        /// Creates a ticket to find opponents. After a successful call, listen for a change in the status of the queue,allocate a playfab server and return its detail .
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        void FindMatchWithServerAllocation(FindMatchRequest request, Action<FindMatchResult> result);
    }
}

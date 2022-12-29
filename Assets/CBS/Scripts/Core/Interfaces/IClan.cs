using CBS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS
{
    public interface IClan
    {
        /// <summary>
        /// Notifies when a user has created a clan.
        /// </summary>
        event Action<CreateClanResult> OnCreateClan;
        /// <summary>
        /// Notifies when a user has joined a clan
        /// </summary>
        event Action<AcceptClanInviteResult> OnJoinClan;
        /// <summary>
        /// Notifies when a user has left the clan.
        /// </summary>
        event Action<LeaveClanResult> OnLeaveClan;
        /// <summary>
        /// Notifies when a user has deleted a clan.
        /// </summary>
        event Action<RemoveClanResult> OnRemoveClan;
        /// <summary>
        /// Notifies when a user has been accepted into a clan.
        /// </summary>
        event Action<AcceptDeclineUserRequestResult> OnUserAccepted;
        /// <summary>
        /// Notifies when a clan invitation has been declined for a user.
        /// </summary>
        event Action<AcceptDeclineUserRequestResult> OnUserDeclined;
        /// <summary>
        /// Notifies when a user has decline clan invation
        /// </summary>
        event Action<DeclineInviteResult> OnUserDeclineClanInvation;

        /// <summary>
        /// Checks if the current user is a member of the clan. It also returns short information about the clan if it is a member.
        /// </summary>
        /// <param name="result"></param>
        void ExistInClan(Action<ExistInClanResult> result);
        /// <summary>
        /// Checks if the user is a member of the clan. It also returns short information about the clan if it is a member. Entity ID is used for the request, not to be confused with PlayerID.
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="result"></param>
        void ExistInClan(string entityID, Action<ExistInClanResult> result);
        /// <summary>
        /// Creation of a new clan.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        void CreateClan(CreateClanRequest request, Action<CreateClanResult> result);
        /// <summary>
        /// Get full information about the clan.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        void GetClanInfo(string clanID, Action<GetClanInfoResult> result);
        /// <summary>
        /// Get a list of all existing clans in the game.
        /// </summary>
        /// <param name="result"></param>
        void GetClanList(Action<GetClanListResult> result);
        /// <summary>
        /// Find a clan by name. Full name is required.
        /// </summary>
        /// <param name="clanName"></param>
        /// <param name="result"></param>
        void SearchClanByName(string clanName, Action<GetClanListResult> result);
        /// <summary>
        /// Send an application to join the clan.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        void JoinClanRequest(string clanID, Action<JoinClanResult> result);
        /// <summary>
        /// Leave the current clan if you are a member.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        void LeaveClan(string clanID, Action<LeaveClanResult> result);
        /// <summary>
        /// Delete a clan. This can only be done by the administrator / creator of the clan.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="clanName"></param>
        /// <param name="result"></param>
        void RemoveClan(string clanID, string clanName, Action<RemoveClanResult> result);
        /// <summary>
        /// Send an invitation to a user to join a clan.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="userEntityID"></param>
        /// <param name="result"></param>
        void InviteUser(string clanID, string userEntityID, Action<InviteToClanResult> result);
        /// <summary>
        /// Get a list of all invitations of the current user to join the clan.
        /// </summary>
        /// <param name="result"></param>
        void GetUserInvitations(Action<GetUserInvatitaionsResult> result);
        /// <summary>
        /// Decline clan invitation to join.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        void DeclineClanInvite(string clanID, Action<DeclineInviteResult> result);
        /// <summary>
        /// Accept the clan's invitation to join.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        void AcceptClanInvitatiion(string clanID, Action<AcceptClanInviteResult> result);
        /// <summary>
        /// Update the description of the clan the user is currently a member of
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="description"></param>
        /// <param name="result"></param>
        void UpdateClanDescription(string clanID, string description, Action<UpdateClanDataResult> result);
        /// <summary>
        /// Update the link to the avatar of the clan in which the user is currently a member.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="url"></param>
        /// <param name="result"></param>
        void UpdateClanImageURL(string clanID, string url, Action<UpdateClanDataResult> result);
        /// <summary>
        /// Set clan clan custom data by specific id.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="dataKey"></param>
        /// <param name="dataValue"></param>
        /// <param name="result"></param>
        void SetOrUpdateClanData(string clanID, string dataKey, string dataValue, Action<UpdateClanDataResult> result);
        /// <summary>
        /// Get clan clan custom data by specific id.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="dataKey"></param>
        /// <param name="result"></param>
        void GetClanCustomData(string clanID, string dataKey, Action<GetClanDataResult> result);
        /// <summary>
        /// Get a list of all users who want to join the clan.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        void GetClanUsersJoinRequests(string clanID, Action<GetClanRequestedUsersResult> result);
        /// <summary>
        /// Accept the user's request to join the clan.
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        void AcceptUserJoinRequest(string entityID, string clanID, Action<AcceptDeclineUserRequestResult> result);
        /// <summary>
        /// Decline the user's request to join the clan
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        void DeclineUserJoinRequest(string entityID, string clanID, Action<AcceptDeclineUserRequestResult> result);
        /// <summary>
        /// Get a list of all clan members.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        void GetClanMemberships(string clanID, Action<GetClanMembershipsResult> result);
        /// <summary>
        /// Remove a member from the clan.
        /// </summary>
        /// <param name="userEntityID"></param>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        void RemoveClanMember(string userEntityID, string clanID, Action<RemoveClanMemberResult> result);
    }
}

using PlayFab;
using PlayFab.GroupsModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public interface IFabGroup
    {
        void GetGroupList(EntityKey entity, Action<ListMembershipResponse> onGet, Action<PlayFabError> onFailed);

        void SendJoinRequest(EntityKey entityToJoin, EntityKey groupToJoin, Action<ApplyToGroupResponse> onJoin, Action<PlayFabError> onFailed);

        void LeaveGroup(EntityKey entityToLeave, EntityKey group, Action<EmptyResponse> onRemove, Action<PlayFabError> onFailed);

        void InviteToGroup(EntityKey entityToInvite, EntityKey group, Action<InviteToGroupResponse> onInvite, Action<PlayFabError> onFailed);

        void GetUserInvitationList(EntityKey playerEntity, Action<ListMembershipOpportunitiesResponse> onGet, Action<PlayFabError> onFailed);

        void DeclineInvation(EntityKey entityToDecline, EntityKey group, Action<EmptyResponse> onGet, Action<PlayFabError> onFailed);

        void GetGroupApplication(EntityKey group, Action<ListGroupApplicationsResponse> onGet, Action<PlayFabError> onFailed);

        void AcceptGroupApplication(EntityKey entityToAccept, EntityKey group, Action<EmptyResponse> onAccept, Action<PlayFabError> onFailed);

        void RemoveGroupApplication(EntityKey entityToDecline, EntityKey group, Action<EmptyResponse> onDecline, Action<PlayFabError> onFailed);

        void RemoveGroupMember(EntityKey entityToRemove, EntityKey group, Action<EmptyResponse> onRemove, Action<PlayFabError> onFailed);
    }
}
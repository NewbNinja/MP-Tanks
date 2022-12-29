using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.GroupsModels;
using System;

namespace CBS.Playfab
{
    public class FabGroup : FabExecuter, IFabGroup
    {
        public void GetGroupList(EntityKey entity, Action<ListMembershipResponse> onGet, Action<PlayFabError> onFailed)
        {
            var request = new ListMembershipRequest {
                Entity = entity
            };

            PlayFabGroupsAPI.ListMembership(request, onGet, onFailed);
        }

        public void SendJoinRequest(EntityKey entityToJoin, EntityKey groupToJoin, Action<ApplyToGroupResponse> onJoin, Action<PlayFabError> onFailed)
        {
            var request = new ApplyToGroupRequest
            {
                Entity = entityToJoin,
                Group = groupToJoin,
                AutoAcceptOutstandingInvite = false
            };

            PlayFabGroupsAPI.ApplyToGroup(request, onJoin, onFailed);
        }

        public void LeaveGroup(EntityKey entityToLeave, EntityKey group, Action<EmptyResponse> onRemove, Action<PlayFabError> onFailed)
        {
            var memberList = new List<EntityKey>();
            memberList.Add(entityToLeave);

            var request = new RemoveMembersRequest { 
                Group = group,
                Members = memberList
            };
            PlayFabGroupsAPI.RemoveMembers(request, onRemove, onFailed);
        }

        public void InviteToGroup(EntityKey entityToInvite, EntityKey group, Action<InviteToGroupResponse> onInvite, Action<PlayFabError> onFailed)
        {
            var request = new InviteToGroupRequest {
                Entity = entityToInvite,
                Group = group
            };
            PlayFabGroupsAPI.InviteToGroup(request, onInvite, onFailed);
        }

        public void GetUserInvitationList(EntityKey playerEntity, Action<ListMembershipOpportunitiesResponse> onGet, Action<PlayFabError> onFailed)
        {
            var requsest = new ListMembershipOpportunitiesRequest{ 
                Entity = playerEntity
            };
            PlayFabGroupsAPI.ListMembershipOpportunities(requsest, onGet, onFailed);
        }

        public void DeclineInvation(EntityKey entityToDecline, EntityKey group, Action<EmptyResponse> onGet, Action<PlayFabError> onFailed)
        {
            var requsest = new RemoveGroupInvitationRequest
            {
                Entity = entityToDecline,
                Group = group
            };
            PlayFabGroupsAPI.RemoveGroupInvitation(requsest, onGet, onFailed);
        }

        public void GetGroupApplication(EntityKey group, Action<ListGroupApplicationsResponse> onGet, Action<PlayFabError> onFailed)
        {
            var request = new ListGroupApplicationsRequest {
                Group = group
            };
            PlayFabGroupsAPI.ListGroupApplications(request, onGet, onFailed);
        }

        public void AcceptGroupApplication(EntityKey entityToAccept, EntityKey group, Action<EmptyResponse> onAccept, Action<PlayFabError> onFailed)
        {
            var request = new AcceptGroupApplicationRequest {
                Entity = entityToAccept,
                Group = group
            };
            PlayFabGroupsAPI.AcceptGroupApplication(request, onAccept, onFailed);
        }

        public void RemoveGroupApplication(EntityKey entityToDecline, EntityKey group, Action<EmptyResponse> onDecline, Action<PlayFabError> onFailed)
        {
            var request = new RemoveGroupApplicationRequest {
                Entity = entityToDecline,
                Group = group
            };
            PlayFabGroupsAPI.RemoveGroupApplication(request, onDecline, onFailed);
        }

        public void RemoveGroupMember(EntityKey entityToRemove, EntityKey group, Action<EmptyResponse> onRemove, Action<PlayFabError> onFailed)
        {
            var membersList = new List<EntityKey>();
            membersList.Add(entityToRemove);

            var request = new RemoveMembersRequest
            {
                Group = group,
                Members = membersList
            };
            PlayFabGroupsAPI.RemoveMembers(request, onRemove, onFailed);
        }
    }
}

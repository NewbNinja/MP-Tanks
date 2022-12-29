using CBS.Playfab;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.GroupsModels;
using System.Linq;
using PlayFab;

namespace CBS
{
    public class CBSClan : CBSModule, IClan
    {
        /// <summary>
        /// Notifies when a user has created a clan.
        /// </summary>
        public event Action<CreateClanResult> OnCreateClan;
        /// <summary>
        /// Notifies when a user has joined a clan
        /// </summary>
        public event Action<AcceptClanInviteResult> OnJoinClan;
        /// <summary>
        /// Notifies when a user has left the clan.
        /// </summary>
        public event Action<LeaveClanResult> OnLeaveClan;
        /// <summary>
        /// Notifies when a user has deleted a clan.
        /// </summary>
        public event Action<RemoveClanResult> OnRemoveClan;
        /// <summary>
        /// Notifies when a user has been accepted into a clan.
        /// </summary>
        public event Action<AcceptDeclineUserRequestResult> OnUserAccepted;
        /// <summary>
        /// Notifies when a clan invitation has been declined for a user.
        /// </summary>
        public event Action<AcceptDeclineUserRequestResult> OnUserDeclined;
        /// <summary>
        /// Notifies when a user has decline clan invation
        /// </summary>
        public event Action<DeclineInviteResult> OnUserDeclineClanInvation;

        private IFabGroup FabGroup { get; set; }
        private IFabClan FabClan { get; set; }
        private IFabAzure FabAzure { get; set; }
        private IProfile Profile { get; set; }
        private IFabEntity FabEntity { get; set; }

        protected override void Init()
        {
            FabGroup = FabExecuter.Get<FabGroup>();
            FabClan = FabExecuter.Get<FabClan>();
            FabAzure = FabExecuter.Get<FabAzure>();
            FabEntity = FabExecuter.Get<FabEntity>();
            Profile = Get<CBSProfile>();
        }

        // API calls

        /// <summary>
        /// Checks if the current user is a member of the clan. It also returns short information about the clan if it is a member.
        /// </summary>
        /// <param name="result"></param>
        public void ExistInClan(Action<ExistInClanResult> result)
        {
            string entityID = Profile.EntityID;

            FabClan.GetUserClan(entityID, onGet => {
                if (onGet.Error != null)
                {
                    result?.Invoke(new ExistInClanResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawData = onGet.FunctionResult.ToString();
                    var callbackObject = JsonUtility.FromJson<ExistInClanCallback>(rawData);

                    result?.Invoke(new ExistInClanResult
                    {
                        IsSuccess = true,
                        ExistInClan = callbackObject.ExistInClan,
                        ClanID = callbackObject.ClanID,
                        ClanName = callbackObject.ClanName
                    });
                }
            }, onError => {
                result?.Invoke(new ExistInClanResult { 
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                });
            });
        }

        /// <summary>
        /// Checks if the user is a member of the clan. It also returns short information about the clan if it is a member. Entity ID is used for the request, not to be confused with PlayerID.
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="result"></param>
        public void ExistInClan(string entityID, Action<ExistInClanResult> result)
        {
            FabClan.GetUserClan(entityID, onGet => {
                if (onGet.Error != null)
                {
                    result?.Invoke(new ExistInClanResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawData = onGet.FunctionResult.ToString();
                    var callbackObject = JsonUtility.FromJson<ExistInClanCallback>(rawData);

                    result?.Invoke(new ExistInClanResult
                    {
                        IsSuccess = true,
                        ExistInClan = callbackObject.ExistInClan,
                        ClanID = callbackObject.ClanID,
                        ClanName = callbackObject.ClanName
                    });
                }
            }, onError => {
                result?.Invoke(new ExistInClanResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                });
            });
        }

        /// <summary>
        /// Get full information about the clan.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        public void GetClanInfo(string clanID, Action<GetClanInfoResult> result)
        {
            FabClan.GetClanInfo(clanID, onGet => {
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetClanInfoResult {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawData = onGet.FunctionResult.ToString();
                    var clanInfo = JsonUtility.FromJson<ClanInfo>(rawData);
                    
                    result?.Invoke(new GetClanInfoResult
                    {
                        IsSuccess = true,
                        Info = clanInfo
                    });
                }
            }, onFailed => {
                result?.Invoke(new GetClanInfoResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Creation of a new clan.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        public void CreateClan(CreateClanRequest request, Action<CreateClanResult> result)
        {
            var entity = new EntityKey
            {
                Id = Profile.EntityID,
                Type = Profile.EntityType
            };
            request.PlayerEntity = entity;

            FabClan.CreateClan(request, onCreate => {
                if (onCreate.Error != null)
                {
                    result?.Invoke(new CreateClanResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onCreate.Error)
                    });
                }
                else
                {
                    string clanID = onCreate.FunctionResult.ToString();
                    Debug.Log(onCreate.FunctionResult);
                    var createCallback = new CreateClanResult
                    {
                        IsSuccess = true,
                        ClanID = clanID
                    };
                    result?.Invoke(createCallback);
                    OnCreateClan?.Invoke(createCallback);
                }
            }, onFailed => {
                result?.Invoke(new CreateClanResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Send an application to join the clan.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        public void SearchClanByName(string clanName, Action<GetClanListResult> result)
        {
            var request = new AzureGetDataRequest
            {
                nTop = CBSConstants.MaxClanFetchCount,
                TableId = CBSConstants.ClansTableID,
                RowKey = clanName
            };

            FabAzure.GetDataFromTable(request, onGet => {
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetClanListResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    Debug.Log(onGet.FunctionResult.ToString());
                    string rawData = onGet.FunctionResult.ToString();
                    var clanCallback = JsonUtility.FromJson<CutClanInfo>(rawData);
                    var resultDict = new Dictionary<string, string>();
                    resultDict.Add(clanCallback.ClanID, clanCallback.RowKey);

                    result?.Invoke(new GetClanListResult
                    {
                        IsSuccess = true,
                        Clans = resultDict
                    });
                }
            }, onFailed => {
                result?.Invoke(new GetClanListResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Find a clan by name. Full name is required.
        /// </summary>
        /// <param name="clanName"></param>
        /// <param name="result"></param>
        public void GetClanList(Action<GetClanListResult> result)
        {
            var request = new AzureGetDataRequest {
                nTop = CBSConstants.MaxClanFetchCount,
                TableId = CBSConstants.ClansTableID,
            };

            FabAzure.GetDataFromTable(request, onGet => { 
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetClanListResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    string rawData = onGet.FunctionResult.ToString();
                    var clanCallback = JsonUtility.FromJson<ClanListCallback>(rawData);
                    var resultDict = new Dictionary<string, string>();
                    foreach (var info in clanCallback.value)
                    {
                        resultDict.Add(info.ClanID, info.RowKey);
                    }
                    result?.Invoke(new GetClanListResult { 
                        IsSuccess = true,
                        Clans = resultDict
                    });
                }
            }, onFailed => {
                result?.Invoke(new GetClanListResult { 
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Leave the current clan if you are a member.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        public void JoinClanRequest(string clanID, Action<JoinClanResult> result)
        {
            var playerEntity = new EntityKey
            {
                Id = Profile.EntityID,
                Type = Profile.EntityType
            };

            var clanEntity = new EntityKey
            {
                Id = clanID,
                Type = CBSConstants.ClanEntityType
            };

            FabGroup.SendJoinRequest(playerEntity, clanEntity, onJoin => {
                result?.Invoke(new JoinClanResult
                {
                    IsSuccess = true,
                    ClanID = clanID
                });
            }, onFailed => {
                result?.Invoke(new JoinClanResult {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Delete a clan. This can only be done by the administrator / creator of the clan.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="clanName"></param>
        /// <param name="result"></param>
        public void LeaveClan(string clanID, Action<LeaveClanResult> result)
        {
            var playerEntity = new EntityKey
            {
                Id = Profile.EntityID,
                Type = Profile.EntityType
            };

            var clanEntity = new EntityKey
            {
                Id = clanID,
                Type = CBSConstants.ClanEntityType
            };

            FabGroup.LeaveGroup(playerEntity, clanEntity, onJoin => {
                var leaveCallback = new LeaveClanResult
                {
                    IsSuccess = true,
                    ClanID = clanID
                };
                result?.Invoke(leaveCallback);
                OnLeaveClan?.Invoke(leaveCallback);
            }, onFailed => {
                result?.Invoke(new LeaveClanResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Delete a clan. This can only be done by the administrator / creator of the clan.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="clanName"></param>
        /// <param name="result"></param>
        public void RemoveClan(string clanID, string clanName, Action<RemoveClanResult> result)
        {
            FabClan.RemoveClan(clanID, clanName, onRemove => {
                if (onRemove.Error != null)
                {
                    result?.Invoke(new RemoveClanResult {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onRemove.Error)
                    });
                }
                else
                {
                    var resultCallback = new RemoveClanResult
                    {
                        IsSuccess = true,
                        ClanID = clanID
                    };
                    result?.Invoke(resultCallback);
                    OnRemoveClan?.Invoke(resultCallback);
                }
            }, onFailed => {
                result?.Invoke(new RemoveClanResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Send an invitation to a user to join a clan.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="userEntity"></param>
        /// <param name="result"></param>
        public void InviteUser(string clanID, string userEntity, Action<InviteToClanResult> result)
        {
            var playerEntity = new EntityKey
            {
                Id = userEntity,
                Type = Profile.EntityType
            };

            var clanEntity = new EntityKey
            {
                Id = clanID,
                Type = CBSConstants.ClanEntityType
            };

            FabGroup.InviteToGroup(playerEntity, clanEntity, onInvite => {
                result?.Invoke(new InviteToClanResult
                {
                    IsSuccess = true,
                    UserEntity = userEntity,
                    Expires = onInvite.Expires
                });
            }, onFailed => {
                result?.Invoke(new InviteToClanResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Get a list of all invitations of the current user to join the clan.
        /// </summary>
        /// <param name="result"></param>
        public void GetUserInvitations(Action<GetUserInvatitaionsResult> result)
        {
            var playerEntity = new EntityKey
            {
                Id = Profile.EntityID,
                Type = Profile.EntityType
            };

            FabGroup.GetUserInvitationList(playerEntity, onGet => {
                var list = onGet.Invitations;
                var callbackList = new List<InvitationInfo>();
                foreach (var info in list)
                {
                    callbackList.Add(new InvitationInfo { 
                        ClanID = info.Group.Id,
                        Expires = info.Expires
                    });
                }
                result?.Invoke(new GetUserInvatitaionsResult { 
                    IsSuccess = true,
                    Invites = callbackList
                });
            }, onError => {
                result?.Invoke(new GetUserInvatitaionsResult { 
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                });
            });
        }

        /// <summary>
        /// Decline clan invitation to join.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        public void DeclineClanInvite(string clanID, Action<DeclineInviteResult> result)
        {
            var playerEntity = new EntityKey
            {
                Id = Profile.EntityID,
                Type = Profile.EntityType
            };

            var clanEntity = new EntityKey
            {
                Id = clanID,
                Type = CBSConstants.ClanEntityType
            };

            FabGroup.DeclineInvation(playerEntity, clanEntity, onDecline => {
                var declineResult = new DeclineInviteResult
                {
                    IsSuccess = true,
                    ClanID = clanID
                };
                OnUserDeclineClanInvation?.Invoke(declineResult);
                result?.Invoke(declineResult);
            }, onFailed => {
                result?.Invoke(new DeclineInviteResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Accept the clan's invitation to join.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        public void AcceptClanInvitatiion(string clanID, Action<AcceptClanInviteResult> result)
        {
            string playerEntity = Profile.EntityID;

            FabClan.AcceptClanInvite(clanID, playerEntity, onAccept => { 
                if (onAccept.Error != null)
                {
                    result?.Invoke(new AcceptClanInviteResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onAccept.Error)
                    });
                }
                else
                {
                    var joinCallback = new AcceptClanInviteResult
                    {
                        IsSuccess = true,
                        ClanID = clanID
                    };
                    result?.Invoke(joinCallback);
                    OnJoinClan?.Invoke(joinCallback);
                }
            }, onFailed => {
                result?.Invoke(new AcceptClanInviteResult { 
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Update the description of the clan the user is currently a member of
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="description"></param>
        /// <param name="result"></param>
        public void UpdateClanDescription(string clanID, string description, Action<UpdateClanDataResult> result)
        {
            string dataKey = CBSConstants.ClanDescriptionDataKey;

            SetOrUpdateClanData(clanID, dataKey, description, result);
        }

        /// <summary>
        /// Update the link to the avatar of the clan in which the user is currently a member.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="url"></param>
        /// <param name="result"></param>
        public void UpdateClanImageURL(string clanID, string url, Action<UpdateClanDataResult> result)
        {
            string dataKey = CBSConstants.ClanImageURLDataKey;

            SetOrUpdateClanData(clanID, dataKey, url, result);
        }

        /// <summary>
        /// Set clan clan custom data by specific id.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="dataKey"></param>
        /// <param name="dataValue"></param>
        /// <param name="result"></param>
        public void SetOrUpdateClanData(string clanID, string dataKey, string dataValue, Action<UpdateClanDataResult> result)
        {
            var entity = new PlayFab.DataModels.EntityKey
            {
                Id = clanID,
                Type = CBSConstants.ClanEntityType
            };

            FabEntity.SetObject(entity, dataKey, dataValue, onUpdate => {
                result?.Invoke(new UpdateClanDataResult
                {
                    IsSuccess = true,
                    ClanID = clanID
                });
            }, onFailed => {
                result?.Invoke(new UpdateClanDataResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Get clan clan custom data by specific id.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="dataKey"></param>
        /// <param name="result"></param>
        public void GetClanCustomData(string clanID, string dataKey, Action<GetClanDataResult> result)
        {
            var entity = new PlayFab.DataModels.EntityKey
            {
                Id = clanID,
                Type = CBSConstants.ClanEntityType
            };

            FabEntity.GetObjects(entity, onGet => {
                var objects = onGet.Objects;
                var data = objects.ContainsKey(dataKey) ? objects[dataKey] : null;
                string value = data == null ? string.Empty : data.DataObject.ToString();

                result?.Invoke(new GetClanDataResult
                {
                    IsSuccess = true,
                    Value = value
                });
            }, onFailed => {
                result?.Invoke(new GetClanDataResult { 
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Get a list of all users who want to join the clan.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        public void GetClanUsersJoinRequests(string clanID, Action<GetClanRequestedUsersResult> result)
        {
            FabClan.GetClanApplications(clanID, onGet => { 
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetClanRequestedUsersResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawData = onGet.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var rawResult = jsonPlugin.DeserializeObject<ClanApplicationCallback>(rawData);

                    result?.Invoke(new GetClanRequestedUsersResult
                    {
                        IsSuccess = true,
                        Profiles = rawResult.Profiles
                    });
                }
            }, onFailed => {
                result?.Invoke(new GetClanRequestedUsersResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Accept the user's request to join the clan.
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        public void AcceptUserJoinRequest(string entityID, string clanID, Action<AcceptDeclineUserRequestResult> result)
        {
            FabClan.AcceptGroupApplication(entityID, clanID, onAccept => {
                if (onAccept.Error != null)
                {
                    result?.Invoke(new AcceptDeclineUserRequestResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onAccept.Error)
                    });
                }
                else
                {
                    var resultCallback = new AcceptDeclineUserRequestResult
                    {
                        IsSuccess = true,
                        UserEntityID = entityID
                    };
                    result?.Invoke(resultCallback);
                    OnUserAccepted?.Invoke(resultCallback);
                }
                
            }, onFailed => {
                result?.Invoke(new AcceptDeclineUserRequestResult {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Decline the user's request to join the clan
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        public void DeclineUserJoinRequest(string entityID, string clanID, Action<AcceptDeclineUserRequestResult> result)
        {
            var playerEntity = new EntityKey
            {
                Id = entityID,
                Type = Profile.EntityType
            };

            var clanEntity = new EntityKey
            {
                Id = clanID,
                Type = CBSConstants.ClanEntityType
            };

            FabGroup.RemoveGroupApplication(playerEntity, clanEntity, onDecline => {
                var resultCallback = new AcceptDeclineUserRequestResult
                {
                    IsSuccess = true,
                    UserEntityID = entityID
                };
                result?.Invoke(resultCallback);
                OnUserDeclined?.Invoke(resultCallback);
            }, onFailed => {
                result?.Invoke(new AcceptDeclineUserRequestResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Get a list of all clan members.
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        public void GetClanMemberships(string clanID, Action<GetClanMembershipsResult> result)
        {
            FabClan.GetClanMemberShips(clanID, onGet => {
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetClanMembershipsResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawData = onGet.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var rawResult = jsonPlugin.DeserializeObject<ClanMembersCallback>(rawData);

                    result?.Invoke(new GetClanMembershipsResult
                    {
                        IsSuccess = true,
                        Profiles = rawResult.Profiles
                    });
                }
            }, onFailed => {
                result?.Invoke(new GetClanMembershipsResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Remove a member from the clan.
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="clanID"></param>
        /// <param name="result"></param>
        public void RemoveClanMember(string entityID, string clanID, Action<RemoveClanMemberResult> result)
        {
            var playerEntity = new EntityKey
            {
                Id = entityID,
                Type = Profile.EntityType
            };

            var clanEntity = new EntityKey
            {
                Id = clanID,
                Type = CBSConstants.ClanEntityType
            };

            FabGroup.RemoveGroupMember(playerEntity, clanEntity, onRemove => {
                result?.Invoke(new RemoveClanMemberResult
                {
                    IsSuccess = true
                });
            }, onFalied => {
                result?.Invoke(new RemoveClanMemberResult {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFalied)
                });
            });
        }
    }

    [Serializable]
    internal class ClanListCallback
    {
        public List<CutClanInfo> value;
    }

    [Serializable]
    internal class CutClanInfo
    {
        public string PartitionKey;
        public string RowKey;
        public string Timestamp;
        public string ClanID;
    }

    public class InvitationInfo
    {
        public string ClanID;
        public DateTime Expires;
    }

    public struct ExistInClanResult
    {
        public bool IsSuccess;
        public SimpleError Error;

        public bool ExistInClan;
        public string ClanID;
        public string ClanName;
    }

    public struct CreateClanRequest
    {
        public string ClanName;
        public string ClanDescription;
        public string ClanImageURL;

        internal EntityKey PlayerEntity;
    }

    public struct CreateClanResult
    {
        public bool IsSuccess;
        public SimpleError Error;

        public string ClanID;
    }

    public struct GetClanListResult
    {
        public bool IsSuccess;
        public SimpleError Error;

        public Dictionary<string, string> Clans;
    }

    public struct JoinClanResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string ClanID;
    }

    public struct LeaveClanResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string ClanID;
    }

    public struct RemoveClanResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string ClanID;
    }

    public struct GetClanInfoResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public ClanInfo Info;
    }

    public struct InviteToClanResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public DateTime Expires;
        public string UserEntity;
    }

    public struct GetUserInvatitaionsResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public List<InvitationInfo> Invites;
    }

    public struct DeclineInviteResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string ClanID;
    }

    public struct AcceptClanInviteResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string ClanID;
    }

    public struct UpdateClanDataResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string ClanID;
    }

    public struct GetClanDataResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string Value;
    }

    public struct GetClanRequestedUsersResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public ClanRequestUser[] Profiles;
    }

    public struct GetClanMembershipsResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public ClanUser[] Profiles;
    }

    public struct RemoveClanMemberResult
    {
        public bool IsSuccess;
        public SimpleError Error;
    }

    public struct AcceptDeclineUserRequestResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string UserEntityID;
    }
}

using CBS.Playfab;
using CBS.Scriptable;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS
{
    public class CBSProfile : CBSModule, IProfile
    {
        /// <summary>
        /// An event that reports when the username has been updated.
        /// </summary>
        public event Action<CBSUpdateDisplayNameResult> OnDisplayNameUpdated;
        /// <summary>
        /// An event that reports when the username has been updated avatar URL.
        /// </summary>
        public event Action<CBSUpdateAvatarUrlResult> OnAvatarImageUpdated;
        /// <summary>
        /// An event that reports when information about the current user has been received.
        /// </summary>
        public event Action<CBSGetAccountInfoResult> OnAcountInfoGetted;
        /// <summary>
        /// An event that reports when the current player's experience points have been changed.
        /// </summary>
        public event Action<CBSUpdateLevelDataResult> OnPlayerExperienceUpdated;

        private IAuth Auth { get; set; }

        private IFabAccount PlayfabData { get; set; }

        private IFabTitle FabTitle { get; set; }

        private AuthData AuthData { get; set; }

        // profile public data

        /// <summary>
        /// Unique user identifier.
        /// </summary>
        public string PlayerID { get; private set; }

        /// <summary>
        /// Display name of current user.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Registration date of the current user.
        /// </summary>
        public string RegistrationDate { get; private set; }

        /// <summary>
        /// Entity ID of current user. Used by Playfab for new features such as groups for example.
        /// </summary>
        public string EntityID { get; private set; }

        /// <summary>
        /// Entity type of current user. For profile its always "title_player_account"
        /// </summary>
        public string EntityType { get; private set; }

        /// <summary>
        /// Avatar url of current user
        /// </summary>
        public string AvatarUrl { get; private set; }

        /// <summary>
        /// Cached Entity key
        /// </summary>
        public EntityKey EntityKey => new EntityKey { Id = EntityID, Type = EntityType };

        /// <summary>
        /// Last cached user level information
        /// </summary>
        public LevelInfo CacheLevelInfo { get; private set; } = new LevelInfo();

        /// <summary>
        /// Player session data
        /// </summary>
        public static PlayFabAuthenticationContext AuthenticationContex { get; private set; }


        protected override void Init()
        {
            AuthData = CBSScriptable.Get<AuthData>();
            Auth = Get<CBSAuth>();
            PlayfabData = FabExecuter.Get<FabAccount>();
            FabTitle = FabExecuter.Get<FabTitleData>();

            Auth.OnLoginEvent += OnUserLogined;
        }

        // API calls

        /// <summary>
        /// Update player display name
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="result"></param>
        public void UpdateUserName(string userName, Action<CBSUpdateDisplayNameResult> result = null)
        {
            PlayfabData.UpdateUserDisplayName(userName, onUpdate => {

                DisplayName = userName;
                CBSUpdateDisplayNameResult callback = new CBSUpdateDisplayNameResult { 
                    IsSuccess = true,
                    DisplayName = DisplayName
                };

                result?.Invoke(callback);
                OnDisplayNameUpdated?.Invoke(callback);
            }, onFailed => {

                CBSUpdateDisplayNameResult callback = new CBSUpdateDisplayNameResult { 
                    IsSuccess = false, 
                    Error = SimpleError.FromTemplate(onFailed)
                };

                OnDisplayNameUpdated?.Invoke(callback);
                result?.Invoke(callback);
            });
        }

        /// <summary>
        /// Get full information of current player account. Include all Playfab origin result.
        /// </summary>
        /// <param name="result"></param>
        public void GetAccountInfo(Action<CBSGetAccountInfoResult> result)
        {
            PlayfabData.GetUserAccountInfo(PlayerID, onSuccess => {
                ParseAccountInfo(onSuccess.AccountInfo);

                var callbackData = new CBSGetAccountInfoResult
                {
                    IsSuccess = true,
                    Result = onSuccess.AccountInfo,
                    DisplayName = DisplayName,
                    AvatarUrl = onSuccess.AccountInfo.TitleInfo.AvatarUrl
                };

                OnAcountInfoGetted?.Invoke(callbackData);
                result?.Invoke(callbackData);
            }, onError => {
                var failedResult = new CBSGetAccountInfoResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                };

                result?.Invoke(failedResult);
            });
        }

        /// <summary>
        /// Get full information of player account by id. Include all Playfab origin result.
        /// </summary>
        /// <param name="result"></param>
        public void GetUserAccountInfo(string userID, Action<CBSGetAccountInfoResult> result)
        {
            PlayfabData.GetUserAccountInfo(userID, onSuccess => {
                string nickname = onSuccess.AccountInfo.TitleInfo.DisplayName;
                Debug.Log(nickname);
                var callbackData = new CBSGetAccountInfoResult
                {
                    IsSuccess = true,
                    Result = onSuccess.AccountInfo,
                    DisplayName = nickname
                };
                result?.Invoke(callbackData);
            }, onError => {
                var failedResult = new CBSGetAccountInfoResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                };

                result?.Invoke(failedResult);
            });
        }

        /// <summary>
        /// Adds N points of experience to the current state. In the response, you can get information whether the player has reached a new level and also information about the reward about the new level.
        /// </summary>
        /// <param name="expToAdd"></param>
        /// <param name="result"></param>
        public void AddPlayerExp(int expToAdd, Action<CBSUpdateLevelDataResult> result = null)
        {
            PlayfabData.AddPlayerExpirience(expToAdd, onSuccess => {

                if (onSuccess.Error == null)
                {
                    var rawData = onSuccess.FunctionResult.ToString();

                    var levelData = ParseLevelData(rawData);

                    var callbackData = new CBSUpdateLevelDataResult
                    {
                        IsSuccess = true,
                        LevelInfo = levelData
                    };

                    if (levelData.NewLevelReached)
                    {
                        if (levelData.NewLevelPrize != null && levelData.NewLevelPrize.BundledVirtualCurrencies != null && levelData.NewLevelPrize.BundledVirtualCurrencies.Count > 0)
                        {
                            var currencyList = levelData.NewLevelPrize.BundledVirtualCurrencies;
                            foreach (var cc in currencyList)
                            {
                                Get<CBSCurrency>().ChangeRequest(cc.Key);
                            }
                        }
                    }

                    OnPlayerExperienceUpdated?.Invoke(callbackData);
                    result?.Invoke(callbackData);
                }
                else
                {
                    var failedResult = new CBSUpdateLevelDataResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onSuccess.Error)
                    };

                    result?.Invoke(failedResult);
                }
            }, onError => {

                var failedResult = new CBSUpdateLevelDataResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                };

                result?.Invoke(failedResult);
            });
        }

        /// <summary>
        /// Get information about current experience and player level.
        /// </summary>
        /// <param name="result"></param>
        public void GetPlayerLevelData(Action<CBSGetLevelDataResult> result)
        {
            PlayfabData.GetPlayerExpirience(PlayerID, onSuccess => {
                if (onSuccess.Error == null)
                {
                    var resultObject = onSuccess.FunctionResult;
                    var rawData = resultObject == null ? string.Empty : resultObject.ToString();
                    var levelData = ParseLevelData(rawData);

                    var callbackData = new CBSGetLevelDataResult
                    {
                        IsSuccess = true,
                        LevelInfo = levelData
                    };

                    result?.Invoke(callbackData);
                }
                else
                {
                    var failedResult = new CBSGetLevelDataResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onSuccess.Error)
                    };

                    result?.Invoke(failedResult);
                }
            }, onError => {

                var failedResult = new CBSGetLevelDataResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                };

                result?.Invoke(failedResult);
            });
        }

        /// <summary>
        /// Get an array with information about all levels in the game.
        /// </summary>
        /// <param name="result"></param>
        public void GetLevelTable(Action<CBSGetLevelTableResult> result)
        {
            string dataKey = CBSConstants.LevelTitleKey;
            FabTitle.GetTitleData(dataKey, onSuccess => {
                var dictionary = onSuccess.Data;
                string rawData = dictionary.ContainsKey(dataKey) ? dictionary[dataKey] : string.Empty;
                var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                var table = jsonPlugin.DeserializeObject<LevelTable>(rawData);

                result?.Invoke(new CBSGetLevelTableResult {
                    IsSuccess = true,
                    Levels = table
                });
            }, onError => {
                result?.Invoke(new CBSGetLevelTableResult {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                });
            });
        }

        /// <summary>
        /// Get general game information about a player, including player ID, avatar url, display name, player level and clan information.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        public void GetPlayerProfile(CBSGetProfileRequest request, Action<CBSGetProfileResult> result)
        {
            PlayfabData.GetProfile(request.ProfileID, request.LoadLevel, request.LoadClan, request.LoadEntityId, onGet => {
                
                if (onGet.Error != null)
                {
                    result?.Invoke(new CBSGetProfileResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawData = onGet.FunctionResult.ToString();
                    
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var rawObject = jsonPlugin.DeserializeObject<ProfileData>(rawData);
                    var clanRaw = rawObject.ClanData == null ? string.Empty : rawObject.ClanData.ToString();
                    var clanData = string.IsNullOrEmpty(clanRaw) ? new ExistInClanCallback() : jsonPlugin.DeserializeObject<ExistInClanCallback>(clanRaw);
                    var levelRaw = rawObject.LevelData == null ? string.Empty : rawObject.LevelData.ToString();
                    var levelData = ParseLevelData(levelRaw);

                    result?.Invoke(new CBSGetProfileResult
                    {
                        IsSuccess = true,
                        ProfileID = rawObject.ProfileID,
                        AvatarURL = rawObject.AvatarUrl,
                        DisplayName = rawObject.DisplayName,
                        LevelData = levelData,
                        ClanData = clanData,
                        EntityID = rawObject.EntityID
                    });
                }
            }, onFailed => {
                result?.Invoke(new CBSGetProfileResult { 
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Get the specific player data by unique key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="result"></param>
        public void GetProfileData(string key, Action<CBSGetProfileDataResult> result)
        {
            GetPlayerData(PlayerID, new string[] { key }, result);
        }

        /// <summary>
        /// Get the specific player data by unique keys.
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="result"></param>
        public void GetProfileData(string [] keys, Action<CBSGetProfileDataResult> result)
        {
            GetPlayerData(PlayerID, keys, result);
        }

        /// <summary>
        /// Get the custom player data by user id
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="key"></param>
        /// <param name="result"></param>
        public void GetProfileDataByPlayerID(string playerID, string key, Action<CBSGetProfileDataResult> result)
        {
            GetPlayerData(playerID, new string[] { key }, result);
        }

        /// <summary>
        /// Get the custom player data by user id
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="keys"></param>
        /// <param name="result"></param>
        public void GetProfileDataByPlayerID(string playerID, string [] keys, Action<CBSGetProfileDataResult> result)
        {
            GetPlayerData(playerID, keys, result);
        }

        /// <summary>
        /// Set/Save custom player data of current profile. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        public void SaveProfileData(string key, string value, Action<CBSSaveProfileDataResult> result)
        {
            PlayfabData.SavePlayerData(key, value, onSave => {
                result?.Invoke(new CBSSaveProfileDataResult {
                    IsSuccess = true
                });
            }, onFailed=>{
                result?.Invoke(new CBSSaveProfileDataResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Update the current player's profile photo.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="result"></param>
        public void UpdateAvatarUrl(string url, Action<CBSUpdateAvatarUrlResult> result)
        {
            PlayfabData.SetAvatarUrl(url, onUpdate => {
                AvatarUrl = url;
                var updateResult = new CBSUpdateAvatarUrlResult
                {
                    IsSuccess = true,
                    Url = url
                };
                result?.Invoke(updateResult);
                OnAvatarImageUpdated?.Invoke(updateResult);
            }, onFailed => {
                result?.Invoke(new CBSUpdateAvatarUrlResult { 
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        // internal
        private void GetPlayerData(string profileID, string[] keys, Action<CBSGetProfileDataResult> result)
        {
            PlayfabData.GetPlayerData(profileID, keys, onGet =>
            {
                result?.Invoke(new CBSGetProfileDataResult
                {
                    IsSuccess = true,
                    Data = onGet.Data.ToDictionary(x => x.Key, x => x.Value.Value)
                });
            },
            onFailed => {
                result?.Invoke(new CBSGetProfileDataResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        internal LevelInfo ParseLevelData(string rawData)
        {
            var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
            LevelInfo levelInfo = string.IsNullOrEmpty(rawData) ? new LevelInfo() : jsonPlugin.DeserializeObject<LevelInfo>(rawData);
            CacheLevelInfo = levelInfo;
            return levelInfo;
        }

        internal void ParseLoginResult(CBSLoginResult resultData)
        {
            var loginResult = resultData.Result;
            var profile = loginResult.InfoResultPayload.PlayerProfile;
            AvatarUrl = profile == null ? string.Empty : profile.AvatarUrl;
            PlayerID = loginResult.PlayFabId;
            EntityID = loginResult.EntityToken.Entity.Id;
            EntityType = loginResult.EntityToken.Entity.Type;
            AuthenticationContex = loginResult.AuthenticationContext;
        }

        internal void ParseAccountInfo(UserAccountInfo resultData)
        {
            var profileData = resultData.TitleInfo;
            AvatarUrl = profileData.AvatarUrl;
            DisplayName = profileData.DisplayName;
            RegistrationDate = profileData.Created.ToString();
        }

        protected override void OnLogout()
        {
            CacheLevelInfo = new LevelInfo();
            PlayerID = string.Empty;
            DisplayName = string.Empty;
            RegistrationDate = string.Empty;
            AvatarUrl = string.Empty;
            EntityID = string.Empty;
            EntityType = string.Empty;
            AuthenticationContex = null;
        }

        // events
        private void OnUserLogined(CBSLoginResult result)
        {
            if (result.IsSuccess)
            {
                ParseLoginResult(result);
                bool preloadAccountInfo = AuthData.PreloadAccountInfo;
                if (preloadAccountInfo)
                {
                    ParseAccountInfo(result.Result.InfoResultPayload.AccountInfo);
                }
            }
        }
    }

    [Serializable]
    public struct CBSGetLevelDataResult
    {
        public bool IsSuccess;
        public LevelInfo LevelInfo;
        public SimpleError Error;
    }

    [Serializable]
    public struct CBSUpdateLevelDataResult
    {
        public bool IsSuccess;
        public LevelInfo LevelInfo;
        public SimpleError Error;
    }

    public struct CBSUpdateDisplayNameResult
    {
        public bool IsSuccess;
        public string DisplayName;
        public SimpleError Error;
    }

    public struct CBSGetAccountInfoResult
    {
        public bool IsSuccess;
        public string DisplayName;
        public string AvatarUrl;
        public UserAccountInfo Result;
        public SimpleError Error;
    }

    public struct CBSUpdatePlayerExpResult
    {
        public bool IsSuccess;
        public ExecuteCloudScriptResult Result;
        public SimpleError Error;
    }

    public struct CBSGetLevelTableResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public LevelTable Levels;
    }

    public struct CBSGetProfileResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string ProfileID;
        public string EntityID;
        public string AvatarURL;
        public string DisplayName;
        public LevelInfo LevelData;
        public ExistInClanCallback ClanData;
    }

    public struct CBSGetProfileRequest
    {
        public string ProfileID;
        public bool LoadLevel;
        public bool LoadClan;
        public bool LoadEntityId;
    }

    public struct CBSUpdateAvatarUrlResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string Url;
    }

    public struct CBSSaveProfileDataResult
    {
        public bool IsSuccess;
        public SimpleError Error;
    }

    public struct CBSGetProfileDataResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public Dictionary<string, string> Data;
    }

}

using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS
{
    public interface IProfile
    {
        /// <summary>
        /// An event that reports when the username has been updated.
        /// </summary>
        event Action<CBSUpdateDisplayNameResult> OnDisplayNameUpdated;
        /// <summary>
        /// An event that reports when information about the current user has been received.
        /// </summary>
        event Action<CBSGetAccountInfoResult> OnAcountInfoGetted;
        /// <summary>
        /// An event that reports when the current player's experience points have been changed.
        /// </summary>
        event Action<CBSUpdateLevelDataResult> OnPlayerExperienceUpdated;
        /// <summary>
        /// An event that reports when the username has been updated avatar URL.
        /// </summary>
        event Action<CBSUpdateAvatarUrlResult> OnAvatarImageUpdated;

        /// <summary>
        /// Unique user identifier.
        /// </summary>
        string PlayerID { get; }

        /// <summary>
        /// Display name of current user.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Registration date of the current user.
        /// </summary>
        string RegistrationDate { get; }

        /// <summary>
        /// Entity ID of current user. Used by Playfab for new features such as groups for example.
        /// </summary>
        string EntityID { get; }

        /// <summary>
        /// Entity type of current user. For profile its always "title_player_account"
        /// </summary>
        string EntityType { get; }

        /// <summary>
        /// Avatar url of current user
        /// </summary>
        string AvatarUrl { get; }

        /// <summary>
        /// Cached Entity key
        /// </summary>
        EntityKey EntityKey { get; }

        /// <summary>
        /// Last cached user level information
        /// </summary>
        LevelInfo CacheLevelInfo { get; }

        /// <summary>
        /// Update player display name
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="result"></param>
        void UpdateUserName(string userName, Action<CBSUpdateDisplayNameResult> result = null);

        /// <summary>
        /// Get full information of current player account. Include all Playfab origin result.
        /// </summary>
        /// <param name="result"></param>
        void GetAccountInfo(Action<CBSGetAccountInfoResult> result);

        /// <summary>
        /// Get full information of player account by id. Include all Playfab origin result.
        /// </summary>
        /// <param name="result"></param>
        void GetUserAccountInfo(string userID, Action<CBSGetAccountInfoResult> result);

        /// <summary>
        /// Adds N points of experience to the current state. In the response, you can get information whether the player has reached a new level and also information about the reward about the new level.
        /// </summary>
        /// <param name="expToAdd"></param>
        /// <param name="result"></param>
        void AddPlayerExp(int expToAdd, Action<CBSUpdateLevelDataResult> result = null);

        /// <summary>
        /// Get information about current experience and player level.
        /// </summary>
        /// <param name="result"></param>
        void GetPlayerLevelData(Action<CBSGetLevelDataResult> result);

        /// <summary>
        /// Get an array with information about all levels in the game.
        /// </summary>
        /// <param name="result"></param>
        void GetLevelTable(Action<CBSGetLevelTableResult> result);

        /// <summary>
        /// Get general game information about a player, including player ID, avatar url, display name, player level and clan information.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        void GetPlayerProfile(CBSGetProfileRequest request, Action<CBSGetProfileResult> result);

        /// <summary>
        /// Update the current player's profile photo.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="result"></param>
        void UpdateAvatarUrl(string url, Action<CBSUpdateAvatarUrlResult> result);

        /// <summary>
        /// Get the specific player data of current profile by unique key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="result"></param>
        void GetProfileData(string key, Action<CBSGetProfileDataResult> result);

        /// <summary>
        /// Get the specific player data of current profile by unique keys.
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="result"></param>
        void GetProfileData(string[] keys, Action<CBSGetProfileDataResult> result);

        /// <summary>
        /// Get the custom player data by user id
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="key"></param>
        /// <param name="result"></param>
        void GetProfileDataByPlayerID(string playerID, string key, Action<CBSGetProfileDataResult> result);

        /// <summary>
        /// Get the custom player data by user id
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="keys"></param>
        /// <param name="result"></param>
        void GetProfileDataByPlayerID(string playerID, string[] keys, Action<CBSGetProfileDataResult> result);

        /// <summary>
        /// Set/Save custom player data of current profile. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        void SaveProfileData(string key, string value, Action<CBSSaveProfileDataResult> result);
    }
}

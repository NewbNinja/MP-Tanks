using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CBS
{
    public static class AzureFunctions
    {
        // cloud
        public const string AddExperienceMethod = "AddPlayerExperience";
        public const string AddCurrencyMethod = "AddVirtualCurrency";
        public const string DecreaseCurrencyMethod = "DecreaseVirtualCurrency";
        public const string FindMatchMethod = "FindMatch";
        public const string CloseRoomVisibilityMethod = "CloseRoomVisibility";
        public const string GetExperienceMethod = "GetPlayerExperienceData";
        public const string GetPlayerProfileMethod = "GetPlayerProfile";
        public const string GrantItemMethod = "GrandItem";
        public const string GrantBundleMethod = "GrandBundle";
        public const string GrantRegistrationPrizeMethod = "GrandRegistrationPrize";
        public const string RemoveInventoryItemMethod = "RemoveInvertoryItem";
        public const string UpdateInventoryItemData = "UpdateInvertoryItemData";
        public const string SendFriendRequestMethod = "SendFriendRequest";
        public const string GetFriendsListMethod = "GetFriendsList";
        public const string AcceptFriendMethod = "AcceptFriend";
        public const string ForceAddFriendMethod = "ForceAddFriend";
        public const string RemoveFriendMethod = "RemoveFriend";
        public const string CreateClanMethod = "CreateClan";
        public const string GetClanMethod = "GetClanInfo";
        public const string RemoveClanMethod = "RemoveClan";
        public const string AcceptClanInviteMethod = "AcceptClanInvite";
        public const string AcceptGroupApplicationMethod = "AcceptGroupApplication";
        public const string GetClanAppicationsMethod = "GetClanAppications";
        public const string GetClanMembershipsMethod = "GetClanMemberships";
        public const string GetUserClanMethod = "GetUserClan";
        public const string GetLeaderboardMethod = "GetLeaderboard";
        public const string GetLeaderboardAroundPlayerMethod = "GetLeaderboardAroundPlayer";
        public const string GetFirendsLeaderboardMethod = "GetFriendsLeaderboard";
        public const string GetClanAdminLeadersMethod = "GetClanLeaders";
        public const string UpdateStatisticMethod = "UpdateStatisticPoint";
        public const string AddStatisticMethod = "AddStatisticPoint";
        public const string AddClanStatisticMethod = "AddClanStatisticPoint";
        public const string UpdateClanStatisticMethod = "UpdateClanStatisticPoint";
        public const string ResetPlayerStatisticsMethod = "ResetPlayerStatistics";
        public const string GetTournamentStateMethod = "GetTournamentState";
        public const string FindAndJoinTournamentMethod = "FindAndJoinTournament";
        public const string LeaveTournamentMethod = "LeaveTournament";
        public const string UpdatePlayerTournamentPointMethod = "UpdatePlayerTournamentPoint";
        public const string AddPlayerTournamentPointMethod = "AddPlayerTournamentPoint";
        public const string FinishTournamentMethod = "FinishTournament";
        public const string GetTournamentMethod = "GetTournament";
        public const string GetAllTournamentMethod = "GetAllTournament";
        public const string GetDailyBonusMethod = "GetDailyBonus";
        public const string GetRouletteTableMethod = "GetRouletteTable";
        public const string SpinRouletteMethod = "SpinRoulette";
        public const string CollectDailyBonusMethod = "CollectDailyBonus";
        public const string ResetDailyBonusMethod = "ResetDailyBonus";
        public const string GetMatchmakingListMethod = "GetMatchmakingList";
        public const string UpdateMatchmakingQueueMethod = "UpdateMatchmakingQueue";
        public const string RemoveMatchmakingQueueMethod = "RemoveMatchmakingQueue";
        public const string GetMatchmakingQueueMethod = "GetMatchmakingQueue";
        public const string GetAchievementsTableMethod = "GetAchievementsTable";
        public const string AddAchievementPointsMethod = "AddAchievementPoints";
        public const string PickupAchievementRewardMethod = "PickupAchievementReward";
        public const string ResetAchievementMethod = "ResetAchievement";
        public const string GetDailyTasksTableMethod = "GetDailyTasksTable";
        public const string GetDailyTasksMethod = "GetDailyTasksState";
        public const string AddDailyTaskPointsMethod = "AddDailyPointsToDailyTask";
        public const string PickupDailyTaskRewardMethod = "PickupDailyTaskTaskReward";
        public const string ResetDailyTasksMethod = "ResetDailyTasksState";
        public const string GetPlayerBattlePassStatesMethod = "GetPlayerBattlePassStates";
        public const string GetBattlePassFullInformationMethod = "GetBattlePassFullInformation";
        public const string AddBattlePassExpirienceMethod = "AddBattlePassExpirience";
        public const string GetRewardFromBattlePassInstanceMethod = "GetRewardFromBattlePassInstance";
        public const string GrantPremiumAccessToBattlePassMethod = "GrantPremiumAccessToBattlePass";
        public const string ResetPlayerStateForBattlePassMethod = "ResetPlayerStateForBattlePass";

        // azure
        public const string AzureGetTableMethod = "GetDataFromTable";
        public const string AzureInsertDataMethod = "InsertDataToTable";
        public const string AzureUpdateDataMethod = "UpdateTableData";
        public const string AzureDeleteDataMethod = "DeleteTableData";
        public const string AzureGetTablesMethod = "GetTables";
        // azure chat
        public const string MessageListUpdateMethod = "UpdateMessageList";
        public const string ClearUnreadMethod = "ClearUnreadMessage";

        // All methods list
        public static List<string> AllMethods
        {
            get
            {
                return typeof(AzureFunctions).GetAllPublicConstantValues<string>();
            }
        }

        private static List<T> GetAllPublicConstantValues<T>(this Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
                .Select(x => (T)x.GetRawConstantValue())
                .ToList();
        }

        public static string GetFunctionFullURL(string funtionUrl, string functionName, string functionMasterKey)
        {
            return string.Format("{0}/api/{1}?code={2}", funtionUrl, functionName, functionMasterKey);
        }
    }
}

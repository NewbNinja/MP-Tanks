using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS
{
    public class CBSConstants
    {
        // statistics
        public const string StaticsticExpKey = "PlayerExp";
        public const string LevelTitleKey = "CBSLevelTable";
        // currency
        public const string CurrencyCatalogID = "CBSCurrency";
        // items
        public const string ItemsCatalogID = "CBSItems";
        // title data
        public const string CategoriesKey = "CBSCategories";
        public const string PackCategoriesKey = "CBSPackCategories";
        public const string LootboxesCategoriesKey = "CBSLutBoxesCategories";
        public const string AzureKey = "CBSAzureKey";
        public const string AzureStorageKey = "CBSAzureStorage";
        public const string FunctionMasterKey = "CBSFunctionsMasterKey";
        public const string FunctionURLKey = "CBSFunctionsURLKey";
        // items
        public const string UndefinedCategory = "undefined";
        // invertory
        public const string InventoryEqvipedKey = "IsEqviped";
        public const string InventoryTradeKey = "IsInTrading";
        public const string InventoryBaseDataKey = "InventoryBaseData";
        // chat
        public const string UnknownName = "Unknown";
        public const string ChatGlobalID = "CBSGlobalChat";
        public const string ChatDefaultRegion = "en";
        public const string ChatDefaultServer = "CBSDefaultServer";
        public const string ChatTablePrefix = "chat";
        public const string ChatListTablePrefix = "chatlist";
        public const int ChatCompareCount = 10;
        public const int ChatCompareWait = 200; // miliseconds
        public const int MaxChatHistory = 100;
        // friends
        public const string FriendRequestTag = "Request";
        public const string FriendAcceptTag = "Accept";
        // clan
        public const string MaxClanMembersKey = "CBSMaxClanMembers";
        public const int MaxClanFetchCount = 100;
        public const string ClansTableID = "cbsclans";
        public const string ClanEntityType = "group";
        public const string MemberRoleID = "members";
        public const string ClanDescriptionDataKey = "Description";
        public const string ClanImageURLDataKey = "ImageURL";
        public const string ClanStatisticKey = "CBSClanRating";
        // leaderboard
        public const int MaxLeaderboardCount = 100;
        public const int MaxClanLeaderboardCount = 20;
        // tournaments
        public const string TournamentsDataKey = "CBSTournaments";
        // daily bonus
        public const string DailyBonusTitleKey = "CBSDailyBonus";
        // roulette
        public const string RouletteTitleKey = "CBSRoulette";
        // entities
        public const string EntityPlayerType = "title_player_account";
        // matchmaking
        public const int MatchmakingDefaultWaitTime = 120; // in seconds
        public const int MatchmakingRefreshTime = 6000; // in miliseconds
        public const string MatchmakingLevelEqualityAttribute = "LevelEquality";
        public const string MatchmakingLevelDifferenceAttribute = "LevelDifference";
        public const string MatchmakingValueDifferenceAttribute = "ValueDifference";
        public const string MatchmakingStringEqualityAttribute = "MatchmakingStringEquality";
        public const string LevelEqualityRuleName = "CBSLevelEquality";
        public const string StringEqualityRuleName = "CBSStringEquality";
        public const string LevelDifferenceRuleName = "CBSLevelDifference";
        public const string ValueDifferenceRuleName = "CBSValueDifference";
        // achievements
        public const string AchievementsTitleKey = "CBSAchievements";
        // daily tasks
        public const string DailyTasksTitleKey = "CBSDailyTasks";
        // battle pass
        public const string BattlePassDataKey = "CBSBattlePass";
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CBS.Editor
{
    public class ResourcesUtils
    {
        private static readonly string TexturePath = "Assets/CBS/Content/Editor/";

        public static Texture GetMenuTexture(MenuTitles title, ButtonState state)
        {
            var imagePath = string.Empty;
            switch (title)
            {
                case MenuTitles.Auth:
                    imagePath = state == ButtonState.Default ? "auth_default.png" : "auth_active.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Profile:
                    imagePath = state == ButtonState.Default ? "profile_default.png" : "profile_active.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Currency:
                    imagePath = state == ButtonState.Default ? "currency_default.png" : "currency_active.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Items:
                    imagePath = state == ButtonState.Default ? "items_default.png" : "items_active.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Azure:
                    imagePath = state == ButtonState.Default ? "azure_default.png" : "azure_active.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Chat:
                    imagePath = state == ButtonState.Default ? "chat_default.png" : "chat_active.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Clans:
                    imagePath = state == ButtonState.Default ? "clan_default.png" : "clan_active.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Tournaments:
                    imagePath = state == ButtonState.Default ? "tournament_default.png" : "tournament_active.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.DailyBonus:
                    imagePath = state == ButtonState.Default ? "dailybonus_default.png" : "dailybonus_active.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Roulette:
                    imagePath = state == ButtonState.Default ? "roulette_default.png" : "roulette_active.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.PlayFab:
                    imagePath = state == ButtonState.Default ? "playfab_default.png" : "playfab_active.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Examples:
                    imagePath = state == ButtonState.Default ? "example_default.png" : "example_active.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Matchmaking:
                    imagePath = state == ButtonState.Default ? "matchmaking_default.png" : "matchmaking_active.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Achievements:
                    imagePath = state == ButtonState.Default ? "achievemets_default.png" : "achievements_active.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.DailyTasks:
                    imagePath = state == ButtonState.Default ? "tasks_default.png" : "tasks_active.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Leaderboards:
                    imagePath = state == ButtonState.Default ? "leaderboards_default.png" : "leaderboards_active.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.BattlePass:
                    imagePath = state == ButtonState.Default ? "battle_pass_default.png" : "battle_pass_active.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                default:
                    return null;
            }
        }

        public static Texture GetTitleTexture(MenuTitles title)
        {
            var imagePath = string.Empty;
            switch (title)
            {
                case MenuTitles.Auth:
                    imagePath = "Titles/auth_title.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Profile:
                    imagePath = "Titles/profile_title.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Currency:
                    imagePath = "Titles/currency_title.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Items:
                    imagePath = "Titles/items_title.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Azure:
                    imagePath = "Titles/azure_title.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Chat:
                    imagePath = "Titles/chat_title.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Clans:
                    imagePath = "Titles/clan_title.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Tournaments:
                    imagePath = "Titles/tournament_title.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.DailyBonus:
                    imagePath = "Titles/dailybonus_title.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Roulette:
                    imagePath = "Titles/roulette_title.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.PlayFab:
                    imagePath = "Titles/playfab_title.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Examples:
                    imagePath = "Titles/exampe_title.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Matchmaking:
                    imagePath = "Titles/matchmaking_title.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Achievements:
                    imagePath = "Titles/achievements_title.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.DailyTasks:
                    imagePath = "Titles/tasks_title.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.Leaderboards:
                    imagePath = "Titles/leaderboards_title.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                case MenuTitles.BattlePass:
                    imagePath = "Titles/battle_pass_title.png";
                    return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + imagePath, typeof(Texture));
                default:
                    return null;
            }
        }

        public static Texture GetBackgroundImage()
        {
            return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + "background2.png", typeof(Texture));
        }

        public static Texture GetMatchImage()
        {
            return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + "Matchmaking/MatchIcon.png", typeof(Texture));
        }

        public static Texture GetLeaderboardImage()
        {
            return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + "Leaderboards/LeaderboardIcon.png", typeof(Texture));
        }

        public static Texture GetTextureByPath(string path)
        {
            return (Texture)AssetDatabase.LoadAssetAtPath(TexturePath + path, typeof(Texture));
        }
    }

    public enum ButtonState
    {
        Default,
        Active
    }

}

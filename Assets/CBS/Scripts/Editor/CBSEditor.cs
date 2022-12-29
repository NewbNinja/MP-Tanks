using CBS.Scriptable;
using CBS.UI;
using PlayFab;
using System;
using UnityEditor;
using UnityEngine;

namespace CBS.Editor
{
    public class CBSEditor : MonoBehaviour
    {
        public static string SERVER_DEFINE_KEY = "ENABLE_PLAYFABSERVER_API";
        public static string ADMIN_DEFINE_KEY = "ENABLE_PLAYFABADMIN_API";

        [MenuItem("CBS/Configurator")]
        static void OpenConfigurator()
        {
            if (!ContainDefine(SERVER_DEFINE_KEY) || !ContainDefine(ADMIN_DEFINE_KEY))
            {
                int option = EditorUtility.DisplayDialogComplex("Warning",
                    "Playfab script define are missing. For the work of the editor, their installation is required. Do you want to install now?",
                    "Yes",
                    "No",
                    string.Empty);
                switch (option)
                {
                    // ok.
                    case 0:
                        var activePlatform = EditorUserBuildSettings.selectedBuildTargetGroup;
                        AddCompileDefine(SERVER_DEFINE_KEY, new BuildTargetGroup[] { activePlatform });
                        AddCompileDefine(ADMIN_DEFINE_KEY, new BuildTargetGroup[] { activePlatform });
                        //TagHelper.AddTag(UIView.CanvasTag);
                        TagHelper.CreateTag(UIView.CanvasTag);
                        break;
                }
            }
            else if (!IsPlayFabConfigurated())
            {
                EditorUtility.DisplayDialog("PlayFab Error", "Playfab not configured. Title id or secret key is empty", "Ok");
            }
            else
            {
                ConfiguratorWindow window = (ConfiguratorWindow)EditorWindow.GetWindow(typeof(ConfiguratorWindow));
                window.maxSize = new Vector2(1280f, 720f);
                window.minSize = window.maxSize;
                window.Show();
            }
        }

        private static bool IsPlayFabConfigurated()
        {
            var titleID = PlayFabSettings.TitleId;
            var secretKey = PlayFabSettings.DeveloperSecretKey;
            return !string.IsNullOrEmpty(titleID) && !string.IsNullOrEmpty(secretKey);
        }

        private static void AddCompileDefine(string newDefineCompileConstant, BuildTargetGroup[] targetGroups = null)
        {
            if (targetGroups == null)
                targetGroups = (BuildTargetGroup[])Enum.GetValues(typeof(BuildTargetGroup));

            foreach (BuildTargetGroup grp in targetGroups)
            {
                if (grp == BuildTargetGroup.Unknown)        //the unknown group does not have any constants location
                    continue;

                string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(grp);
                if (!defines.Contains(newDefineCompileConstant))
                {
                    if (defines.Length > 0)         //if the list is empty, we don't need to append a semicolon first
                        defines += ";";

                    defines += newDefineCompileConstant;
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(grp, defines);
                }
            }
        }

        private static bool ContainDefine(string defineCompileConstant)
        {
            var activePlatform = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(activePlatform);
            return defines.Contains(defineCompileConstant);
        }

        [MenuItem("CBS/Prefabs/Auth")]
        static void OpenAuthPrefab()
        {
            var asset = CBSScriptable.Get<AuthPrefabs>();
            Selection.activeObject = asset;
        }

        [MenuItem("CBS/Prefabs/Chat")]
        static void OpenChatPrefab()
        {
            var asset = CBSScriptable.Get<ChatPrefabs>();
            Selection.activeObject = asset;
        }

        [MenuItem("CBS/Prefabs/Clan")]
        static void OpenClanPrefab()
        {
            var asset = CBSScriptable.Get<ClanPrefabs>();
            Selection.activeObject = asset;
        }

        [MenuItem("CBS/Prefabs/Common")]
        static void OpenCommonPrefab()
        {
            var asset = CBSScriptable.Get<CommonPrefabs>();
            Selection.activeObject = asset;
        }

        [MenuItem("CBS/Prefabs/Currency")]
        static void OpenCurrencyPrefab()
        {
            var asset = CBSScriptable.Get<CurrencyPrefabs>();
            Selection.activeObject = asset;
        }

        [MenuItem("CBS/Prefabs/DailyBonus")]
        static void OpenDailyBonusPrefab()
        {
            var asset = CBSScriptable.Get<DailyBonusPrefabs>();
            Selection.activeObject = asset;
        }

        [MenuItem("CBS/Prefabs/Friends")]
        static void OpenFriendsPrefab()
        {
            var asset = CBSScriptable.Get<FriendsPrefabs>();
            Selection.activeObject = asset;
        }

        [MenuItem("CBS/Prefabs/Inventory")]
        static void OpenInventoryPrefab()
        {
            var asset = CBSScriptable.Get<InventoryPrefabs>();
            Selection.activeObject = asset;
        }

        [MenuItem("CBS/Prefabs/Leaderboards")]
        static void OpenLeaderboardsPrefab()
        {
            var asset = CBSScriptable.Get<LeaderboardPrefabs>();
            Selection.activeObject = asset;
        }

        [MenuItem("CBS/Prefabs/LootBox")]
        static void OpenLootBoxPrefab()
        {
            var asset = CBSScriptable.Get<LootboxPrefabs>();
            Selection.activeObject = asset;
        }

        [MenuItem("CBS/Prefabs/Popups")]
        static void OpenPopupPrefab()
        {
            var asset = CBSScriptable.Get<PopupPrefabs>();
            Selection.activeObject = asset;
        }

        [MenuItem("CBS/Prefabs/Profile")]
        static void OpenProfilePrefab()
        {
            var asset = CBSScriptable.Get<ProfilePrefabs>();
            Selection.activeObject = asset;
        }

        [MenuItem("CBS/Prefabs/Roulette")]
        static void OpenRoulettePrefab()
        {
            var asset = CBSScriptable.Get<RoulettePrefabs>();
            Selection.activeObject = asset;
        }

        [MenuItem("CBS/Prefabs/Shop")]
        static void OpenShopPrefab()
        {
            var asset = CBSScriptable.Get<ShopPrefabs>();
            Selection.activeObject = asset;
        }

        [MenuItem("CBS/Prefabs/Tournament")]
        static void OpenTournamentPrefab()
        {
            var asset = CBSScriptable.Get<TournamentPrefabs>();
            Selection.activeObject = asset;
        }

        [MenuItem("CBS/Prefabs/Matchmaking")]
        static void OpenMatchmakingPrefab()
        {
            var asset = CBSScriptable.Get<MatchmakingPrefabs>();
            Selection.activeObject = asset;
        }

        [MenuItem("CBS/Prefabs/Achievements")]
        static void OpenAchievementsPrefab()
        {
            var asset = CBSScriptable.Get<AchievementsPrefabs>();
            Selection.activeObject = asset;
        }

        [MenuItem("CBS/Prefabs/DailyTasks")]
        static void OpenDailyTasksPrefab()
        {
            var asset = CBSScriptable.Get<DailyTasksPrefabs>();
            Selection.activeObject = asset;
        }

        [MenuItem("CBS/Prefabs/BattlePass")]
        static void OpenBattlePassPrefab()
        {
            var asset = CBSScriptable.Get<BattlePassPrefabs>();
            Selection.activeObject = asset;
        }
    }
}

using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CBS.UI
{
    public class IconsPanel : MonoBehaviour
    {
        [SerializeField]
        private string GameScene;

        public void ShowItemsShop()
        {
            var prefabs = CBSScriptable.Get<ShopPrefabs>();
            var shopPrefab = prefabs.ItemsShop;
            UIView.ShowWindow(shopPrefab);
        }

        public void ShowInvertory()
        {
            var prefabs = CBSScriptable.Get<InventoryPrefabs>();
            var invertoryPrefab = prefabs.Inventory;
            UIView.ShowWindow(invertoryPrefab);
        }

        public void ShowLootBox()
        {
            var prefabs = CBSScriptable.Get<LootboxPrefabs>();
            var lootBoxPrefab = prefabs.LootBoxes;
            UIView.ShowWindow(lootBoxPrefab);
        }

        public void ShowChat()
        {
            var prefabs = CBSScriptable.Get<ChatPrefabs>();
            var chatPrefab = prefabs.ChatWindow;
            UIView.ShowWindow(chatPrefab);
        }

        public void ShowFriends()
        {
            var prefabs = CBSScriptable.Get<FriendsPrefabs>();
            var friendsPrefab = prefabs.FriendsWindow;
            UIView.ShowWindow(friendsPrefab);
        }

        public void ShowClan()
        {
            var prefabs = CBSScriptable.Get<ClanPrefabs>();
            var cbsClan = CBSModule.Get<CBSClan>();
            cbsClan.ExistInClan(onGet => {
                if (onGet.IsSuccess)
                {
                    string clanID = onGet.ClanID;
                    bool existInClan = onGet.ExistInClan;
                    var prefab = existInClan ? prefabs.ClanWindow : prefabs.NoClanWindow;
                    var window = UIView.ShowWindow(prefab);
                    if (existInClan)
                        window.GetComponent<ClanWindow>().DisplayClan(clanID);
                }
                else
                {
                    Debug.Log(onGet.Error.Message);
                }
            });
        }

        public void ShowLeaderboards()
        {
            var prefabs = CBSScriptable.Get<LeaderboardPrefabs>();
            var leaderboardsPrefab = prefabs.LeaderboardsWindow;
            UIView.ShowWindow(leaderboardsPrefab);
        }

        public void ShowTournament()
        {
            var prefabs = CBSScriptable.Get<TournamentPrefabs>();
            var tournamentPrefab = prefabs.TournamentWindow;
            UIView.ShowWindow(tournamentPrefab);
        }

        public void ShowDailyBonus()
        {
            var prefabs = CBSScriptable.Get<DailyBonusPrefabs>();
            var dailyBonusPrefab = prefabs.DailyBonusWindow;
            UIView.ShowWindow(dailyBonusPrefab);
        }

        public void ShowRoulette()
        {
            var prefabs = CBSScriptable.Get<RoulettePrefabs>();
            var roulettePrefab = prefabs.RouletteWindow;
            UIView.ShowWindow(roulettePrefab);
        }

        public void ShowMatchmaking()
        {
            var prefabs = CBSScriptable.Get<MatchmakingPrefabs>();
            var matchmakingPrefab = prefabs.MatchmalingWindow;
            UIView.ShowWindow(matchmakingPrefab);
        }

        public void ShowAchievements()
        {
            var prefabs = CBSScriptable.Get<AchievementsPrefabs>();
            var achievementsWindow = prefabs.AchievementsWindow;
            UIView.ShowWindow(achievementsWindow);
        }

        public void ShowDailyTasks()
        {
            var prefabs = CBSScriptable.Get<DailyTasksPrefabs>();
            var tasksWindow = prefabs.DailyTasksWindow;
            UIView.ShowWindow(tasksWindow);
        }

        public void LoadGame()
        {
            SceneManager.LoadScene(GameScene);
        }
    }
}

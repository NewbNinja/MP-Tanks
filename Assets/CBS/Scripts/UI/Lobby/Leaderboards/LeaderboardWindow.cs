using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class LeaderboardWindow : MonoBehaviour
    {
        [SerializeField]
        private LeaderboardTabListener TabListener;
        [SerializeField]
        private GameObject PlayersLeaderboard;
        [SerializeField]
        private GameObject ClansLeaderboard;
        [SerializeField]
        private GameObject FriendsLeaderboard;

        private void Awake()
        {
            TabListener.OnTabSelected += OnTabSelected;
        }

        private void OnDestroy()
        {
            TabListener.OnTabSelected -= OnTabSelected;
        }

        private void OnEnable()
        {
            var activeTab = TabListener.ActiveTab;
            DisplayTab(activeTab);
        }

        private void OnTabSelected(LeaderboardTabType type)
        {
            DisplayTab(type);
        }

        private void DisplayTab(LeaderboardTabType type)
        {
            PlayersLeaderboard.SetActive(type == LeaderboardTabType.PLAYERS);
            ClansLeaderboard.SetActive(type == LeaderboardTabType.CLANS);
            FriendsLeaderboard.SetActive(type == LeaderboardTabType.FRIENDS);
        }
    }
}

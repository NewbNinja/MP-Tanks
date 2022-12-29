using CBS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class LeaderboardUser : MonoBehaviour, IScrollableItem<PlayerLeaderboardEntry>
    {
        [SerializeField]
        private Image Background;
        [SerializeField]
        private Text DisplayName;
        [SerializeField]
        private PlaceDrawer Place;
        [SerializeField]
        private AvatarDrawer Avatar;
        [SerializeField]
        private Text Value;
        [Header("Colors")]
        [SerializeField]
        private Color DefaultColor;
        [SerializeField]
        private Color ActiveColor;

        public void Display(PlayerLeaderboardEntry data)
        {
            DisplayName.text = data.DisplayName;
            Place.Draw(data.Position);
            Value.text = data.StatValue.ToString();
            var cbsProfile = CBSModule.Get<CBSProfile>();
            string profileID = cbsProfile.PlayerID;
            bool isMine = data.PlayFabId == profileID;
            DisplayName.fontStyle = isMine ? FontStyle.Bold : FontStyle.Normal;
            Value.fontStyle = isMine ? FontStyle.Bold : FontStyle.Normal;
            Background.color = isMine ? ActiveColor : DefaultColor;

            Avatar.LoadProfileAvatar(data.PlayFabId);
            Avatar.SetClickable(data.PlayFabId);
        }
    }
}

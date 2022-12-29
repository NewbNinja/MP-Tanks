using CBS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class LeaderboardClan : MonoBehaviour, IScrollableItem<ClanLeaderboardEntry>
    {
        [SerializeField]
        private Image Background;
        [SerializeField]
        private Text DisplayName;
        [SerializeField]
        private PlaceDrawer Place;
        [SerializeField]
        private ClanAvatarDrawer Avatar;
        [SerializeField]
        private Text Value;
        [Header("Colors")]
        [SerializeField]
        private Color DefaultColor;
        [SerializeField]
        private Color ActiveColor;

        public void Display(ClanLeaderboardEntry data)
        {
            DisplayName.text = data.ClanName;
            Place.Draw(data.Position);
            Value.text = data.StatValue.ToString();
            var clanId = data.ClanId;

            bool isMine = !string.IsNullOrEmpty(data.CurrentClanId) && data.ClanId == data.CurrentClanId;
            DisplayName.fontStyle = isMine ? FontStyle.Bold : FontStyle.Normal;
            Value.fontStyle = isMine ? FontStyle.Bold : FontStyle.Normal;
            Background.color = isMine ? ActiveColor : DefaultColor;

            Avatar.LoadFromClanProfile(clanId);
            Avatar.SetClickable(clanId);
        }
    }
}
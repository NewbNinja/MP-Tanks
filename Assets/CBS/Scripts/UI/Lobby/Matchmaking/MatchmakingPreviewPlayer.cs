using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI.Matchmaking
{
    public class MatchmakingPreviewPlayer : MonoBehaviour
    {
        [SerializeField]
        private AvatarDrawer Avatar;
        [SerializeField]
        private Text DisplayName;

        private IProfile Profile { get; set; }

        private void Awake()
        {
            Profile = CBSModule.Get<CBSProfile>();
        }

        public void DrawUser(CBSMatchmakingPlayer player)
        {
            Debug.LogFormat("Player profile {0}", player.ProfileID);

            Profile.GetPlayerProfile(new CBSGetProfileRequest
            {
                ProfileID = player.ProfileID
            }, 
            OnPlayerInfoGet);
        }

        private void OnPlayerInfoGet(CBSGetProfileResult result)
        {
            if (result.IsSuccess)
            {
                var avatarUrl = result.AvatarURL;
                var profileID = result.ProfileID;
                Avatar.LoadAvatarFromUrl(avatarUrl, profileID);
                DisplayName.text = result.DisplayName;
            }
        }
    }
}

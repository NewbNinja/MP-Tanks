using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class ProfileIcon : MonoBehaviour
    {
        [SerializeField]
        private Text NickNameLabel;
        [SerializeField]
        private Text LevelLabel;
        [SerializeField]
        private Text ExpLabel;
        [SerializeField]
        private Slider ExpSlider;
        [SerializeField]
        private AvatarDrawer Avatar;

        private IProfile CBSProfile { get; set; }
        private ProfilePrefabs Prefabs { get; set; }

        private void Start()
        {
            Prefabs = CBSScriptable.Get<ProfilePrefabs>();
            CBSProfile = CBSModule.Get<CBSProfile>();
            // subscribe to events
            CBSProfile.OnDisplayNameUpdated += OnDisplayNameUpdated;
            CBSProfile.OnPlayerExperienceUpdated += OnPlayerExperienceUpdated;
            CBSProfile.OnAvatarImageUpdated += OnAvatarImageUpdated;
            // try display cache value
            DisplayName();
            DisplayLevelData();
            // get actual data from DB
            CBSProfile.GetAccountInfo(OnAccountInfoGetted);
            CBSProfile.GetPlayerLevelData(OnGetLevelData);

            DrawAvatar();
        }

        private void OnDestroy()
        {
            CBSProfile.OnDisplayNameUpdated -= OnDisplayNameUpdated;
            CBSProfile.OnPlayerExperienceUpdated -= OnPlayerExperienceUpdated;
            CBSProfile.OnAvatarImageUpdated -= OnAvatarImageUpdated;
        }

        private void DisplayName()
        {
            NickNameLabel.text = CBSProfile.DisplayName;
        }

        private void DrawAvatar()
        {
            // draw avatar
            var avatarUrl = CBSProfile.AvatarUrl;
            var profileID = CBSProfile.PlayerID;
            Avatar.LoadAvatarFromUrl(avatarUrl, profileID);
        }

        private void DisplayLevelData()
        {
            var levelData = CBSProfile.CacheLevelInfo;
            LevelLabel.text = levelData.CurrentLevel.ToString();

            int curExp = levelData.CurrentExp;
            int nextExp = levelData.NextLevelExp;
            int prevExp = levelData.PrevLevelExp;
            float expVal = (float)(curExp - prevExp) / (float)(nextExp - prevExp);
            ExpLabel.text = curExp.ToString() + "/" + nextExp.ToString();
            ExpSlider.value = expVal;
        }

        // button click
        public void ShowAccountInfo()
        {
            var windowsPrefab = Prefabs.AccountForm;
            UIView.ShowWindow(windowsPrefab);
        }

        // events
        private void OnPlayerExperienceUpdated(CBSUpdateLevelDataResult result)
        {
            DisplayLevelData();
        }

        private void OnDisplayNameUpdated(CBSUpdateDisplayNameResult result)
        {
            DisplayName();
        }

        private void OnAccountInfoGetted(CBSGetAccountInfoResult result)
        {
            DisplayName();
        }

        private void OnGetLevelData(CBSGetLevelDataResult result)
        {
            DisplayLevelData();
        }

        private void OnAvatarImageUpdated(CBSUpdateAvatarUrlResult obj)
        {
            DrawAvatar();
        }
    }
}

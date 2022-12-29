using CBS.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class AccountInfoForm : MonoBehaviour
    {
        [SerializeField]
        private Text NicknameLabel;
        [SerializeField]
        private Text PlayfabIDLabel;
        [SerializeField]
        private Text RegistrationLabel;
        [SerializeField]
        private Text EntityIDLabel;
        [SerializeField]
        private GameObject UserNamePanel;
        [SerializeField]
        private GameObject EditNamePanel;
        [SerializeField]
        private InputField EditInput;

        private IProfile CBSProfile { get; set; }
        private IAuth Auth { get; set; }

        private void Awake()
        {
            CBSProfile = CBSModule.Get<CBSProfile>();
            Auth = CBSModule.Get<CBSAuth>();
        }

        private void OnEnable()
        {
            DisplayUI();
            CBSProfile.OnDisplayNameUpdated += OnUserNameUpdated;
            CBSProfile.GetAccountInfo(OnAccountInfoGetted);
        }

        private void OnDisable()
        {
            CBSProfile.OnDisplayNameUpdated -= OnUserNameUpdated;
        }

        private void DisplayUI()
        {
            ShowNickname();
            NicknameLabel.text = CBSProfile.DisplayName;
            PlayfabIDLabel.text = CBSProfile.PlayerID;
            RegistrationLabel.text = CBSProfile.RegistrationDate;
            EntityIDLabel.text = CBSProfile.EntityID;
            EditInput.text = string.Empty;
        }

        public void ShowNickname()
        {
            UserNamePanel.SetActive(true);
            EditNamePanel.SetActive(false);
        }

        public void ShowEditName()
        {
            EditInput.text = CBSProfile.DisplayName;
            UserNamePanel.SetActive(false);
            EditNamePanel.SetActive(true);
        }

        public void OnLogout()
        {
            Auth.Logout();
        }

        public void UpdateNickname()
        {
            string newName = EditInput.text;
            // check with empty filed
            if (string.IsNullOrEmpty(newName)){
                new PopupViewer().ShowSimplePopup(new PopupRequest
                {
                    Title = AuthTXTHandler.ErrorTitle,
                    Body = AuthTXTHandler.InvalidInput
                });
                return;
            }
            CBSProfile.UpdateUserName(newName, onComplete => {
                if (!onComplete.IsSuccess)
                {
                    new PopupViewer().ShowFabError(onComplete.Error);
                }
            });
        }

        public void CloseWindow()
        {
            gameObject.SetActive(false);
        }

        private void OnUserNameUpdated(CBSUpdateDisplayNameResult result)
        {
            if (result.IsSuccess)
            {
                NicknameLabel.text = result.DisplayName;
                ShowNickname();
            }
        }

        private void OnAccountInfoGetted(CBSGetAccountInfoResult result)
        {
            DisplayUI();
        }
    }
}

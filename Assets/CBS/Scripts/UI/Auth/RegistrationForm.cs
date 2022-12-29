using CBS.Scriptable;
using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class RegistrationForm : MonoBehaviour
    {
        [SerializeField]
        private InputField MailInput;
        [SerializeField]
        private InputField PasswordInput;
        [SerializeField]
        private InputField PasswordRepeatInput;
        [SerializeField]
        private InputField DisplayNameInput;

        private IAuth Auth { get; set; }
        private AuthPrefabs AuthUIData { get; set; }

        private void Start()
        {
            Auth = CBSModule.Get<CBSAuth>();
            AuthUIData = CBSScriptable.Get<AuthPrefabs>();
        }

        private void OnDisable()
        {
            Clear();
        }

        private void Clear()
        {
            MailInput.text = string.Empty;
            PasswordInput.text = string.Empty;
            PasswordRepeatInput.text = string.Empty;
            DisplayNameInput.text = string.Empty;
        }

        // button events
        public void ShowLogin()
        {
            var loginPrefab = AuthUIData.LoginForm;
            UIView.ShowWindow(loginPrefab);
            HideWindow();
        }

        public void OnRegister()
        {
            bool passInput = ValidInputs();
            if (passInput)
            {
                // get gredentials
                string mail = MailInput.text;
                string password = PasswordInput.text;
                string displayName = DisplayNameInput.text;

                // start register
                var registerRequest = new CBSMailRegistrationRequest
                {
                    Mail = mail,
                    Password = password,
                    DisplayName = displayName
                };

                new PopupViewer().ShowLoadingPopup();
                Auth.RegisterWithMailAndPassword(registerRequest, OnRegistrationComplete);
            }
        }

        private void OnRegistrationComplete(CBSMailRegistrationResult result)
        {
            new PopupViewer().HideLoadingPopup();
            if (result.IsSuccess)
            {
                // registration message
                new PopupViewer().ShowSimplePopup(new PopupRequest
                {
                    Title = AuthTXTHandler.RegisterComplete,
                    Body = AuthTXTHandler.RegistrationMessage,
                    OnOkAction = ShowLogin
                });
            }
            else
            {
                new PopupViewer().ShowSimplePopup(new PopupRequest
                {
                    Title = AuthTXTHandler.ErrorTitle,
                    Body = result.Error.Message
                });
            }
        }

        private void HideWindow()
        {
            gameObject.SetActive(false);
        }

        private bool ValidInputs()
        {
            bool mailValid = !string.IsNullOrEmpty(MailInput.text);
            bool passwordValid = !string.IsNullOrEmpty(PasswordInput.text);
            bool passwordRepeatValid = !string.IsNullOrEmpty(PasswordRepeatInput.text);
            bool nameValid = !string.IsNullOrEmpty(DisplayNameInput.text);

            bool fieldsValid = mailValid & passwordValid & passwordRepeatValid & nameValid;
            if (!fieldsValid)
            {
                new PopupViewer().ShowSimplePopup(new PopupRequest
                {
                    Title = AuthTXTHandler.ErrorTitle,
                    Body = AuthTXTHandler.InvalidInput
                });
                return false;
            }

            bool passwordSame = PasswordInput.text == PasswordRepeatInput.text;
            if (!passwordSame)
            {
                new PopupViewer().ShowSimplePopup(new PopupRequest
                {
                    Title = AuthTXTHandler.ErrorTitle,
                    Body = AuthTXTHandler.InvalidPassword
                });
                return false;
            }

            return true;
        }
    }
}

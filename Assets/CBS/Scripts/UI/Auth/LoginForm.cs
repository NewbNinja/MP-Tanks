using CBS.Scriptable;
using CBS.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class LoginForm : MonoBehaviour
    {
        [SerializeField]
        private InputField MailInput;
        [SerializeField]
        private InputField PasswordInput;
        [SerializeField]
        private InputField CustomIDInput;

        public event Action<CBSLoginResult> OnLogined;

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

        // button click
        public void OnLoginWithMail()
        {
            if (InputValid())
            {
                // get credentials
                string mail = MailInput.text;
                string password = PasswordInput.text;
                // start login
                var loginRequest = new CBSMailLoginRequest 
                {
                    Mail = mail,
                    Password = password
                };
                // login request
                new PopupViewer().ShowLoadingPopup();
                Auth.LoginWithMailAndPassword(loginRequest, OnUserLogined);
            }
            else
            {
                // show error message
                new PopupViewer().ShowSimplePopup(new PopupRequest
                {
                    Title = AuthTXTHandler.ErrorTitle,
                    Body = AuthTXTHandler.InvalidInput
                });
            }
        }

        public void OnLoginWithdeviceID()
        {
            new PopupViewer().ShowLoadingPopup();
            Auth.LoginWithDeviceID(OnUserLogined);
        }

        public void OnLoginWithCustomID()
        {
            string customID = CustomIDInput.text;
            if (!string.IsNullOrEmpty(customID))
            {
                new PopupViewer().ShowLoadingPopup();
                Auth.LoginWithCustomID(customID, OnUserLogined);
            }
        }

        private void OnUserLogined(CBSLoginResult result)
        {
            new PopupViewer().HideLoadingPopup();
            if (result.IsSuccess)
            {
                // goto main screen
                gameObject.SetActive(false);
                OnLogined?.Invoke(result);
            }
            else
            {
                // show error message
                new PopupViewer().ShowFabError(result.Error);
            }
        }

        public bool IsSubscribeToLogin()
        {
            return OnLogined.GetInvocationList().Length != 0;
        }

        public void OnRegistration()
        {
            var registrationPrefab = AuthUIData.RegisterForm;
            UIView.ShowWindow(registrationPrefab);
            HideWindow();
        }

        public void OnFogotPassword()
        {
            var recoveryPrefab = AuthUIData.RecoveryForm;
            UIView.ShowWindow(recoveryPrefab);
            HideWindow();
        }

        // check if inputs is not null
        private bool InputValid()
        {
            bool mailValid = !string.IsNullOrEmpty(MailInput.text);
            bool passwordValid = !string.IsNullOrEmpty(PasswordInput.text);
            return mailValid && passwordValid;
        }

        // reset view
        private void Clear()
        {
            MailInput.text = string.Empty;
            PasswordInput.text = string.Empty;
        }

        private void HideWindow()
        {
            gameObject.SetActive(false);
        }
    }
}

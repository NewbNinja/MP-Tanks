using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace SIS
{
    /// <summary>
    /// Sample UI script for registering new accounts and logging in with PlayFab.
    /// The current implementation makes use of email addresses for creating new users.
    /// </summary>
    public class PlayFabUILogin : MonoBehaviour
    {
        /// <summary>
        /// Scene to load immediately after successfully logging in.
        /// </summary>
        public string nextScene;

        /// <summary>
        /// Loading screen game object to activate between login attempts.
        /// </summary>
        public GameObject loadingScreen;

        [Header("Register")]
        /// <summary>
        /// The panel with all registration controls.
        /// </summary>
        public GameObject registerPanel;

        /// <summary>
        /// The toggle that enables this panel.
        /// </summary>
        public Toggle registerToggle;

        /// <summary>
        /// Email address field to register.
        /// </summary>
        public InputField registerEmail;

        /// <summary>
        /// Password field to register.
        /// </summary>
        public InputField registerPassword;

        [Header("Login")]
        /// <summary>
        /// The panel with all login controls.
        /// </summary>
        public GameObject loginPanel;

        /// <summary>
        /// The toggle that enables this panel.
        /// </summary>
        public Toggle loginToggle;

        /// <summary>
        /// Email address field to log in.
        /// </summary>
        public InputField loginEmail;

        /// <summary>
        /// Password field to log in.
        /// </summary>
        public InputField loginPassword;

        /// <summary>
        /// Button for using login via device identifier. Not supported on all services.
        /// </summary>
        public GameObject deviceLoginButton;

        /// <summary>
        /// Error text displayed in case of login issues.
        /// </summary>
        public Text errorText;

        //PlayerPref key used for storing the latest entered email address
        private const string emailPref = "AccountEmail";


        //pre-load login values
        void Start()
        {
            if (PlayerPrefs.HasKey(emailPref))
            {
                loginToggle.Select();
                loginEmail.text = PlayerPrefs.GetString(emailPref);
            }
            else
                registerToggle.Select();
        }


        void OnEnable()
        {
            PlayFabManager.loginSucceededEvent += OnLoggedIn;
            PlayFabManager.loginFailedEvent += OnLoginFail;
        }


        void OnDisable()
        {
            PlayFabManager.loginSucceededEvent -= OnLoggedIn;
            PlayFabManager.loginFailedEvent -= OnLoginFail;
        }


        //loads the desired scene immediately after loggin in
        private void OnLoggedIn(PlayFab.ClientModels.LoginResult result)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }


        //hides the loading screen in case of failed login, so the user can try again
        private void OnLoginFail(string error)
        {
            loadingScreen.SetActive(false);
            errorText.text = error;
        }


        public void OnSwitchPanel()
        {
            registerPanel.SetActive(registerToggle.isOn);
            loginPanel.SetActive(loginToggle.isOn);
        }


        /// <summary>
        /// Registers a new account with PlayFab, mapped to a UI button.
        /// </summary>
        public void RegisterAccount()
        {
            string inputError = Validate(true);
            if (!string.IsNullOrEmpty(inputError))
            {
                errorText.text = inputError;
                return;
            }

            loadingScreen.SetActive(true);
            PlayerPrefs.SetString(emailPref, registerEmail.text);

            PlayFabManager.RegisterAccount(registerEmail.text, registerPassword.text);
        }


        /// <summary>
        /// Tries to login via email, mapped to a UI button.
        /// </summary>
        public void LoginWithEmail()
        {
            string inputError = Validate(false);
            if (!string.IsNullOrEmpty(inputError))
            {
                errorText.text = inputError;
                return;
            }

            loadingScreen.SetActive(true);
            PlayerPrefs.SetString(emailPref, loginEmail.text);

            PlayFabManager.LoginWithEmail(loginEmail.text, loginPassword.text);
        }


        /// <summary>
        /// Tries to login via device identifier, mapped to a UI button.
        /// </summary>
        public void LoginWithDevice()
        {
            PlayFabManager.GetInstance().LoginWithDevice();
        }


        /// <summary>
        /// Requests a new password, mapped to a UI button.
        /// </summary>
        public void ForgotPassword()
        {
            errorText.text = "";
            if (loginEmail.text.Length == 0)
            {
                errorText.text = "Please enter your email and retry.";
                return;
            }
            
            PlayFabManager.ForgotPassword(loginEmail.text);
        }


        private string Validate(bool IsRegister)
        {
            string email = registerEmail.text;
            string password = registerPassword.text;
            if(!IsRegister)
            {
                email = loginEmail.text;
                password = loginPassword.text;
            }

            if (email.Length == 0|| password.Length == 0)
            {
                return "All fields are required.";
            }

            if (password.Length <= 5)
            {
                return "Password must be longer than 5 characters.";
            }

            string emailPattern = "^[a-zA-Z0-9-_.+]+[@][a-zA-Z0-9-_.]+[.][a-zA-Z]+$";
            Regex regex = new Regex(emailPattern);
            if(!regex.IsMatch(email))
            {
                return "Invalid email.";
            }

            return string.Empty;
        }
    }
}

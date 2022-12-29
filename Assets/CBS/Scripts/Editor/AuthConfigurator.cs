using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CBS.Editor
{
    public class AuthConfigurator : BaseConfigurator
    {
        protected override string Title => "Auth Configurator";

        protected override bool DrawScrollView => true;

        private AuthData AuthData { get; set; }

        public override void Init(MenuTitles title)
        {
            base.Init(title);
            AuthData = CBSScriptable.Get<AuthData>();
        }

        protected override void OnDrawInside()
        {
            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 16;

            GUILayout.Space(10);
            EditorGUILayout.LabelField("General options", titleStyle);
            GUILayout.Space(10);
            // autogenerate nick name
            bool autoGen = EditorGUILayout.Toggle("AutoCreate Display Name", AuthData.AutoGenerateRandomNickname);
            string nickNamePrefix = AuthData.RandomNamePrefix;
            if (autoGen)
            {
                GUILayout.Space(10);
                nickNamePrefix = EditorGUILayout.TextField("Random Name Prefix", AuthData.RandomNamePrefix, new GUILayoutOption[] { GUILayout.Width(400) });
            }
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Enable this option to automatically generate player nicknames after registration. Does not apply to registration via email", MessageType.Info);

            GUILayout.Space(10);
            bool autoLogin = EditorGUILayout.Toggle("Auto Login", AuthData.AutoLogin);
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Enable this option for the system to save the last successful authorization on the device. Allows you to use the CBSAuth.AutoLogin method to log into the game.", MessageType.Info);

            GUILayout.Space(10);
            var deviceIdProvider = (DeviceIdDataProvider)EditorGUILayout.EnumPopup("Device ID Data Provider", AuthData.DeviceIdProvider);
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("The source that provides the deviceID for the login.", MessageType.Info);

            GUILayout.Space(10);
            EditorUtils.DrawUILine(Color.grey, 1, 20);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Preload Data options", titleStyle);
            GUILayout.Space(10);

            bool preloadAccount = EditorGUILayout.Toggle("Preload Account Data", AuthData.PreloadAccountInfo);
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Enable this option to get your account details right after login. For example after login you will have access to the property CBSProfile.DisplayName, CBSProfile.AvatarURL.", MessageType.Info);
            GUILayout.Space(10);

            bool preloadLevelData = EditorGUILayout.Toggle("Preload Level Data", AuthData.PreloadLevelData);
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Enable this option to get data about the current level and experience of the player immediately after login. For example, after login, the CBSProfile.CacheLevelInfo property will be available to you.", MessageType.Info);
            GUILayout.Space(10);

            bool preloadCurrencies = EditorGUILayout.Toggle("Preload Currencies", AuthData.PreloadCurrency);
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Enable this option to get all player Currencies immediately after login. For example, after login, the CBSCurrency.CacheCurrencies property will be available to you.", MessageType.Info);
            GUILayout.Space(10);

            AuthData.PreloadLevelData = preloadLevelData;
            AuthData.PreloadAccountInfo = preloadAccount;
            AuthData.AutoGenerateRandomNickname = autoGen;
            AuthData.RandomNamePrefix = nickNamePrefix;
            AuthData.PreloadCurrency = preloadCurrencies;
            AuthData.AutoLogin = autoLogin;
            AuthData.DeviceIdProvider = deviceIdProvider;

            AuthData.Save();
        }
    }
}

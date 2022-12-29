using CBS.Scriptable;
using CBS.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CBS.Context
{
    public class LobbyContext : MonoBehaviour
    {
        [SerializeField]
        private string LoginScene;

        private ProfilePrefabs ProfilePrefabs { get; set; }
        private CommonPrefabs CommonPrefabs { get; set; }
        private CurrencyPrefabs CurrencyPrefabs { get; set; }

        private IAuth CBSAuth { get; set; }

        private void Start()
        {
            ProfilePrefabs = CBSScriptable.Get<ProfilePrefabs>();
            CommonPrefabs = CBSScriptable.Get<CommonPrefabs>();
            CurrencyPrefabs = CBSScriptable.Get<CurrencyPrefabs>();
            CBSAuth = CBSModule.Get<CBSAuth>();
            Init();
            CBSAuth.OnLogoutEvent += OnUserLogout;
        }

        private void OnDestroy()
        {
            CBSAuth.OnLogoutEvent -= OnUserLogout;
        }

        private void Init()
        {
            // show bottom panel
            var panelPrefab = CommonPrefabs.IconsPanel;
            UIView.ShowWindow(panelPrefab);
            // show profile icon
            var profilePrefab = ProfilePrefabs.ProfileIcon;
            UIView.ShowWindow(profilePrefab);
            // show currencies
            var currencyPrefab = CurrencyPrefabs.CurrencyPanel;
            UIView.ShowWindow(currencyPrefab);
        }

        private void OnUserLogout(CBSLogoutResult result)
        {
            SceneManager.LoadScene(LoginScene);
        }
    }
}

using CBS.Scriptable;
using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class BattlePassWindow : MonoBehaviour
    {
        [SerializeField]
        private Text DisplayName;
        [SerializeField]
        private Text Timer;
        [SerializeField]
        private Text Level;
        [SerializeField]
        private Text Description;
        [SerializeField]
        private BattlePassScroller Scroller;
        [SerializeField]
        private GameObject PremiumButton;

        private string BattlePassID { get; set; }

        private IBattlePass BattlePass { get; set; }
        private bool IsActive { get; set; }

        private BattlePassPrefabs PassPrefabs { get; set; }

        private void Awake()
        {
            BattlePass = CBSModule.Get<CBSBattlePass>();
            PassPrefabs = CBSScriptable.Get<BattlePassPrefabs>();
        }

        public void Load(string battlePassID)
        {
            CleanUI();
            BattlePassID = battlePassID;
            BattlePass.GetBattlePassFullInformation(BattlePassID, OnGetBattlePassInfo);
        }

        private void DrawLevels(List<BattlePassLevelInfo> levels)
        {
            var levelPrefab = PassPrefabs.LevelDrawer;
            Scroller.Spawn(levelPrefab, levels);
        }

        private void CleanUI()
        {
            DisplayName.text = string.Empty;
            Timer.text = string.Empty;
            Level.text = string.Empty;
            Description.text = string.Empty;
            Scroller.HideAll();
            PremiumButton.SetActive(false);
        }

        // button click

        public void CloseWindow()
        {
            gameObject.SetActive(false);
        }

        public void Add10Exp()
        {
            if (!IsActive)
                return;
            BattlePass.AddExpirienceToInstance(BattlePassID, 10, OnAddExpirience);
        }

        public void Add150Exp()
        {
            if (!IsActive)
                return;
            BattlePass.AddExpirienceToInstance(BattlePassID, 150, OnAddExpirience);
        }

        public void UnclockPremiumLine()
        {
            BattlePass.GrantPremiumAccessToPlayer(BattlePassID, OnPemiumAccessGranted);
        }

        // events

        private void OnGetBattlePassInfo(GetBattlePassFullInformationResult result)
        {
            if (result.IsSuccess)
            {
                var instance = result.Instance;
                var playerState = result.PlayerState;
                IsActive = playerState.IsActive;
                // draw info
                DisplayName.text = instance.DisplayName;
                Timer.text = BattlePassUtils.GetFrameTimeLabel(playerState);
                Level.text = playerState.PlayerLevel.ToString();
                Description.text = instance.Description;
                PremiumButton.SetActive(!playerState.PremiumRewardAvailable && playerState.IsActive);
                // draw levels
                var levels = result.GetLevelTreeDetailList();
                DrawLevels(levels);
            }
        }

        private void OnAddExpirience(AddExpirienceToInstanceResult result)
        {
            if (result.IsSuccess)
            {
                BattlePass.GetBattlePassFullInformation(BattlePassID, OnGetBattlePassInfo);
            }
        }

        private void OnPemiumAccessGranted(GrantPremiumAccessResult result)
        {
            if (result.IsSuccess)
            {
                BattlePass.GetBattlePassFullInformation(BattlePassID, OnGetBattlePassInfo);
            }
        }
    }
}

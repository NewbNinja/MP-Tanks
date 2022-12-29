using CBS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class BattlePassLevelSlot : MonoBehaviour, IScrollableItem<BattlePassLevelInfo>
    {
        private readonly float DisabledAlpha = 0;
        private readonly float LockAlpha = 0.6f;
        private readonly float DefaultAlpha = 1f;

        private readonly float ExpandWidthPerReward = 190f;

        [SerializeField]
        private CanvasGroup PremuimGroup;
        [SerializeField]
        private CanvasGroup DefaultGroup;
        [SerializeField]
        private Text LevelLabel;
        [SerializeField]
        private Text ExpToNextLevel;
        [SerializeField]
        private Slider ExpSlider;
        [SerializeField]
        private BattlePassRewardDrawer PremiumRewardDrawer;
        [SerializeField]
        private BattlePassRewardDrawer DefaultRewardDrawer;
        [SerializeField]
        private GameObject CollectPremiumButton;
        [SerializeField]
        private GameObject CollectDefaultButton;

        private RectTransform Rect { get; set; }
        private float DefaultFrameWith { get; set; }

        private string BattlePassID { get; set; }
        private int LevelIndex { get; set; }
        private BattlePassLevelInfo LevelInfo { get; set; }

        private void Awake()
        {
            Rect = GetComponent<RectTransform>();
            DefaultFrameWith = Rect.sizeDelta.x;
        }

        public void Display(BattlePassLevelInfo data)
        {
            LevelInfo = data;
            var levelReward = data.LevelDetail ?? new BattlePassLevel();
            var premiumReward = levelReward.PremiumReward;
            var defaultReward = levelReward.DefaultReward;
            var viewLevel = data.LevelIndex;
            BattlePassID = data.battlePassID;
            LevelIndex = viewLevel;
            // draw premium
            if (premiumReward == null)
            {
                PremuimGroup.alpha = DisabledAlpha;
            }
            else
            {
                var rewardAvailableToCollect = !data.IsPremiumRewardCollected && data.IsPremium && viewLevel <= data.PlayerLevel && data.IsPassActive;
                var rewardIsActive = (data.PlayerLevel < viewLevel || rewardAvailableToCollect) && data.IsPremium && data.IsPassActive;
                PremuimGroup.alpha = rewardIsActive ? DefaultAlpha : LockAlpha;
                CollectPremiumButton.SetActive(rewardAvailableToCollect);
                PremiumRewardDrawer.Display(premiumReward);
            }
            // draw default
            if (defaultReward == null)
            {
                DefaultGroup.alpha = DisabledAlpha;
            }
            else
            {
                var rewardAvailableToCollect = !data.IsDefaultRewardCollected && viewLevel <= data.PlayerLevel && data.IsPassActive;
                var rewardIsActive = (data.PlayerLevel < viewLevel || rewardAvailableToCollect) && data.IsPassActive;
                DefaultGroup.alpha = rewardIsActive ? DefaultAlpha : LockAlpha;
                CollectDefaultButton.SetActive(rewardAvailableToCollect);
                DefaultRewardDrawer.Display(defaultReward);
            }
            // draw exp
            ExpSlider.maxValue = data.ExpStep;
            if (data.PlayerLevel == viewLevel)
            {
                ExpToNextLevel.gameObject.SetActive(true);
                ExpSlider.value = data.ExpOfCurrentLevel;
                ExpToNextLevel.text = string.Format("{0}/{1}", data.ExpOfCurrentLevel.ToString(), data.ExpStep);
            }
            else if (data.PlayerLevel > viewLevel)
            {
                ExpSlider.value = data.ExpStep;
                ExpToNextLevel.gameObject.SetActive(false);
            }
            else
            {
                ExpSlider.value = 0;
                ExpToNextLevel.gameObject.SetActive(false);
            }
            if (data.IsLast)
                ExpSlider.gameObject.SetActive(false);
            // draw level
            LevelLabel.text = data.LevelIndex.ToString();
            // fix width
            var premiumRewardCount = premiumReward == null ? 0 : premiumReward.GetPositionCount();
            var defaultRewardCount = defaultReward == null ? 0 : defaultReward.GetPositionCount();
            var maxRewardCount = Mathf.Max(premiumRewardCount, defaultRewardCount);
            var rewardBasedWidth = maxRewardCount * ExpandWidthPerReward;
            var newWidth = Mathf.Max(DefaultFrameWith, rewardBasedWidth);
            Rect.sizeDelta = new Vector2(newWidth, Rect.sizeDelta.y);
        }

        // button click
        public void GrantPremiumReward()
        {
            CBSModule.Get<CBSBattlePass>().GrantAwardToPlayer(BattlePassID, LevelIndex, true, OnReciveReward);
        }

        public void GrantDefaultReward()
        {
            CBSModule.Get<CBSBattlePass>().GrantAwardToPlayer(BattlePassID, LevelIndex, false, OnReciveReward);
        }

        // event
        private void OnReciveReward(GrandAwardToPlayerResult result)
        {
            if (result.IsSuccess)
            {
                var isPremiumReward = result.IsPremium;
                if (isPremiumReward)
                    LevelInfo.IsPremiumRewardCollected = true;
                else
                    LevelInfo.IsDefaultRewardCollected = true;
                Display(LevelInfo);
            }
            else
            {
                Debug.Log("Failed to grant reward "+result.Error.Message);
            }
        }
    }
}

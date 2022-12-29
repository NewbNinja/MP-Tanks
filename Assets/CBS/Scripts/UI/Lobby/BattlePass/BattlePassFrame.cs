using CBS.Scriptable;
using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class BattlePassFrame : MonoBehaviour
    {
        [SerializeField]
        private Text Title;
        [SerializeField]
        private Text Time;
        [SerializeField]
        private Text CurrentLevel;
        [SerializeField]
        private Text CurrentExp;
        [SerializeField]
        private BattlePassBadge Badge;
        [SerializeField]
        private GameObject LockBlock;

        private string BattlePassID { get; set; }

        private BattlePassPrefabs PassPrefabs { get; set; }

        private void Awake()
        {
            PassPrefabs = CBSScriptable.Get<BattlePassPrefabs>();
        }

        public void Draw(BattlePassUserInfo passUserInfo)
        {
            BattlePassID = passUserInfo.BattlePassID;
            Title.text = passUserInfo.BattlePassName;
            Time.text = BattlePassUtils.GetFrameTimeLabel(passUserInfo);
            CurrentLevel.text = passUserInfo.PlayerLevel.ToString();
            CurrentExp.text = passUserInfo.ExpOfCurrentLevel.ToString()+"/"+passUserInfo.ExpStep.ToString();
            Badge.UpdatePassBadge(passUserInfo.RewardBadgeCount);
            LockBlock.SetActive(!passUserInfo.IsActive);
        }

        public void OpenPassWindow()
        {
            var prefab = PassPrefabs.BattlePassWindow;
            var windowObject = UIView.ShowWindow(prefab);
            windowObject.GetComponent<BattlePassWindow>().Load(BattlePassID);
        }
    }
}

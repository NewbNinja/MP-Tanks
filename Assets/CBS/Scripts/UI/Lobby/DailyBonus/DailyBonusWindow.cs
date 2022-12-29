using CBS.Scriptable;
using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class DailyBonusWindow : MonoBehaviour
    {
        [SerializeField]
        private Button CollectBtn;
        [SerializeField]
        private DailyBonusScroller Scroller;

        private IDailyBonus DailyBonus { get; set; }
        private DailyBonusPrefabs Prefabs { get; set; }

        private List<GameObject> DayObjects { get; set; }

        private void Awake()
        {
            DailyBonus = CBSModule.Get<CBSDailyBonus>();
            Prefabs = CBSScriptable.Get<DailyBonusPrefabs>();
        }

        private void OnEnable()
        {
            CollectBtn.interactable = false;
            LoadDailyBonusData();
        }

        private void LoadDailyBonusData()
        {
            DailyBonus.GetDailyBonus(OnDailyBonusGeted);
        }

        private void DrawItems(List<DailyBonusInfo> prizes)
        {
            var slotPrefab = Prefabs.DailyBonusSlot;
            DayObjects = Scroller.Spawn(slotPrefab, prizes);
        }

        private void MoveToDayPosition(int day)
        {
            var scroll = Scroller.GetScroll();
            var dayRect = DayObjects[day].GetComponent<RectTransform>();
            var dayPosition = scroll.GetSnapToPositionToBringChildIntoView(dayRect);
            scroll.content.localPosition = dayPosition;
        }

        // buttons events
        public void OnCollectClick()
        {
            CollectBtn.interactable = false;
            DailyBonus.CollectDailyBonus(OnDailyBonusCollected);
        }

        // events
        private void OnDailyBonusGeted(GetDailyBonusResult result)
        {
            if (result.IsSuccess)
            {
                var picked = result.Picked;
                CollectBtn.interactable = !picked;

                var items = result.DaliyPrizes;
                DrawItems(items);
                // move to current day
                MoveToDayPosition(result.CurrentDailyIndex);
            }
            else
            {
                new PopupViewer().ShowStackError(result.Error);
            }
        }

        private void OnDailyBonusCollected(CollectDailyBonusResult result)
        {
            if (result.IsSuccess)
            {
                new PopupViewer().ShowSimplePopup(new PopupRequest { 
                    Title = DailyBonusTXTHandler.SUCCESS_COLLECT_TITLE,
                    Body = DailyBonusTXTHandler.SUCCESS_COLLECT_BODY
                });
                LoadDailyBonusData();
            }
            else
            {
                new PopupViewer().ShowStackError(result.Error);
            }
        }
    }
}

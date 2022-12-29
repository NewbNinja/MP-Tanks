using CBS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class DailySlot : MonoBehaviour, IScrollableItem<DailyBonusInfo>
    {
        [SerializeField]
        private Text Title;
        [SerializeField]
        private Image Background;
        [SerializeField]
        private PrizeDrawer PrizeDrawer;
        [SerializeField]
        private GameObject Selected;

        public void Display(DailyBonusInfo data)
        {
            Title.text = data.DayNumber.ToString();
            PrizeDrawer.Display(data.Prize);

            var color = Color.white;
            Selected.SetActive(data.IsCurrent);
        }
    }
}

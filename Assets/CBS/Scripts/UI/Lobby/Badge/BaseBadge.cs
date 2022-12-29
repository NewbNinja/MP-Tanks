using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public abstract class BaseBadge : MonoBehaviour
    {
        [SerializeField]
        private Text Count;
        [SerializeField]
        protected GameObject Back;
        [SerializeField]
        private BadgeUpdateType UpdateLogic;
        [SerializeField]
        private int IntervalMiliseconds = 1000;

        private bool ActiveUpdate { get; set; }

        protected void UpdateCount(int count)
        {
            if (Back != null)
            {
                Back.SetActive(count > 0);
                Count.text = count.ToString();
            }
        }

        protected async void StartUpdateInterval()
        {
            if (UpdateLogic != BadgeUpdateType.UPDATE_WITH_INTERVAL)
                return;
            ActiveUpdate = true;
            while(ActiveUpdate && gameObject != null)
            {
                await Task.Delay(IntervalMiliseconds);
                OnUpdateInterval();
            }
        }

        protected void StopUpdateInterval()
        {
            ActiveUpdate = false;
        }

        protected virtual void OnUpdateInterval()
        {

        }
    }

    public enum BadgeUpdateType
    {
        UPDATE_ONCE,
        UPDATE_WITH_INTERVAL
    }
}
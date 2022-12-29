using CBS.Core;
using CBS.Scriptable;
using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class BaseTaskUI : MonoBehaviour, IScrollableItem<CBSTask>
    {
        [SerializeField]
        private Text Title;
        [SerializeField]
        private Text Description;
        [SerializeField]
        private Text StepsLabel;
        [SerializeField]
        private Text NotifyLabel;
        [SerializeField]
        private Image IconImage;
        [SerializeField]
        private Slider StepsSlider;

        [SerializeField]
        private GameObject IconLock;
        [SerializeField]
        private GameObject CompleteBtn;
        [SerializeField]
        private GameObject LockBtn;
        [SerializeField]
        private GameObject AddPointBt;
        [SerializeField]
        private GameObject RewardBt;

        protected CBSTask Task { get; set; }
        private TasksIcons Icons { get; set; }

        protected virtual string LockText { get; }
        protected virtual string NotCompleteText { get; }
        protected virtual string CompleteText { get; }


        private void Awake()
        {
            OnInit();
        }

        protected virtual void OnInit()
        {
            Icons = CBSScriptable.Get<TasksIcons>();
        }

        public void Display(CBSTask task)
        {
            Task = task;
            var id = Task.ID;
            var isCompleted = Task.IsComplete;
            var getReward = Task.Rewarded;

            ToDefault();
            // draw title
            Title.text = Task.Title;
            Description.text = Task.Description;
            // draw sprite
            var iconSprite = Icons.GetSprite(id);
            IconImage.gameObject.SetActive(iconSprite != null);
            IconImage.sprite = iconSprite;
            // check locked
            bool isLocked = !Task.IsAvailable;
            LockBtn.SetActive(isLocked);
            IconLock.SetActive(isLocked);
            if (isLocked)
                NotifyLabel.text = LockText;
            // draw slider state
            if (!isLocked && Task.Type == TaskType.ONE_SHOT)
            {
                NotifyLabel.text = NotCompleteText;
            }
            else if (Task.Type == TaskType.STEPS && !isLocked)
            {
                StepsSlider.gameObject.SetActive(true);
                StepsSlider.maxValue = Task.Steps;
                StepsSlider.value = Task.CurrentStep;
                var sliderTitle = string.Format("{0}/{1}", Task.CurrentStep, Task.Steps);
                StepsLabel.text = sliderTitle;
                StepsSlider.gameObject.SetActive(!isCompleted);
            }
            // draw buttons
            if (isCompleted)
                NotifyLabel.text = CompleteText;

            AddPointBt.SetActive(!isLocked && !isCompleted);
            LockBtn.SetActive(isLocked && !isCompleted);
            RewardBt.SetActive(!isLocked && isCompleted && !getReward);
            CompleteBtn.SetActive(!isLocked && isCompleted && getReward);
        }

        // button click
        public virtual void OnAddPoint() { }

        public virtual void GetRewards() { }

        public void ShowRewards()
        {
            var reward = Task.Reward;
            new PopupViewer().ShowRewardPopup(reward);
        }

        private void ToDefault()
        {
            Title.text = string.Empty;
            Description.text = string.Empty;
            StepsLabel.text = string.Empty;
            NotifyLabel.text = string.Empty;

            IconImage.gameObject.SetActive(false);
            StepsSlider.gameObject.SetActive(false);
            IconLock.SetActive(false);
            CompleteBtn.SetActive(false);
            LockBtn.SetActive(false);
            AddPointBt.SetActive(false);
        }
    }
}

using CBS.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class TimestampTimer : MonoBehaviour
    {
        [SerializeField]
        private Text TimerLabel;

        public event Action OnTimerEnd;

        private int CurrentTime { get; set; }

        private void OnDisable()
        {
            StopTimer();
        }

        public void StartTimer(int timerstamp)
        {
            CurrentTime = timerstamp;
            StopAllCoroutines();
            StartCoroutine(OnTick());
        }

        private IEnumerator OnTick()
        {
            while(true)
            {
                var timeSpan = TimeSpan.FromMilliseconds(CurrentTime);
                var days = timeSpan.Days;
                var timeString = timeSpan.ToString(TournamnetTXTHandler.TimerFormat);
                TimerLabel.text = days > 0 ? days + " Days " + timeString : timeString;
                yield return new WaitForSeconds(1);
                CurrentTime -= 1000;
                if (CurrentTime < 0)
                {
                    CurrentTime = 0;
                    OnTimerEnd?.Invoke();
                    yield break;
                }
            }
        }

        public void StopTimer()
        {
            TimerLabel.text = TournamnetTXTHandler.DefaultTimer;
            StopAllCoroutines();
        }
    }
}

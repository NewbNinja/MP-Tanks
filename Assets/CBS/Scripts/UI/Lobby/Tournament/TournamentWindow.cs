using CBS.Scriptable;
using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class TournamentWindow : MonoBehaviour
    {
        [SerializeField]
        private TournamentLeaderboard Leaderboard;
        [SerializeField]
        private GameObject JoinButton;
        [SerializeField]
        private GameObject LeaveButton;
        [SerializeField]
        private GameObject FinishButton;
        [SerializeField]
        private GameObject Timer;
        [SerializeField]
        private Text TournamentTitle;

        private TournamentPrefabs Prefabs { get; set; }

        private ITournament Tournament { get; set; }

        private void Awake()
        {
            Tournament = CBSModule.Get<CBSTournament>();
            Prefabs = CBSScriptable.Get<TournamentPrefabs>();
            Timer.GetComponent<TimestampTimer>().OnTimerEnd += OnTournamentEnd;
        }

        private void OnDestroy()
        {
            Timer.GetComponent<TimestampTimer>().OnTimerEnd -= OnTournamentEnd;
        }

        private void OnEnable()
        {
            ReDrawState();
        }

        private void ReDrawState()
        {
            ClearState();
            Tournament.GetPlayerCurrentTournamentState(OnGetTournamentState);
        }

        // events
        private void OnGetTournamentState(GetTournamentStateResult result)
        {
            if (result.IsSuccess)
            {
                if (!result.Joined)
                {
                    DrawNotJoinedState();
                }
                else if (result.Finished)
                {
                    Leaderboard.gameObject.SetActive(true);
                    Leaderboard.DisplayLeaderboard(result);

                    DrawFinishedState();
                }
                else
                {
                    Leaderboard.gameObject.SetActive(true);
                    Leaderboard.DisplayLeaderboard(result);

                    DrawActiveState(result.TimeLeft);
                }
                TournamentTitle.text = result.TournamentName;
            }
            else
            {
                new PopupViewer().ShowStackError(result.Error);
            }
        }

        private void OnJoinedTounament(JoinTournamentResult result)
        {
            if (result.IsSuccess)
            {
                ReDrawState();
            }
            else
            {
                new PopupViewer().ShowStackError(result.Error);
            }
        }

        private void OnTournamentEnd()
        {
            ReDrawState();
        }

        private void DrawNotJoinedState()
        {
            JoinButton.SetActive(true);
        }

        private void DrawActiveState(int timeLeft)
        {
            LeaveButton.SetActive(true);
            Timer.SetActive(true);
            Timer.GetComponent<TimestampTimer>().StartTimer(timeLeft);
        }

        private void DrawFinishedState()
        {
            FinishButton.SetActive(true);
        }

        private void ClearState()
        {
            JoinButton.SetActive(false);
            Leaderboard.gameObject.SetActive(false);
            LeaveButton.SetActive(false);
            Timer.SetActive(false);
            FinishButton.SetActive(false);
            TournamentTitle.text = string.Empty;
            Leaderboard.Clear();
            Timer.GetComponent<TimestampTimer>().StopTimer();
        }

        private void ProccessLeaveTournament()
        {
            Tournament.LeaveCurrentTournament(onLeft => { 
                if (onLeft.IsSuccess)
                {
                    ReDrawState();
                }
                else
                {
                    new PopupViewer().ShowStackError(onLeft.Error);
                }
            });
        }

        // button click

        public void JoinTournament()
        {
            Tournament.FindAndJoinTournament(OnJoinedTounament);
        }

        public void LeaveTournament()
        {
            new PopupViewer().ShowYesNoPopup(new YesNoPopupRequest
            {
                Title = TournamnetTXTHandler.WarningTitle,
                Body = TournamnetTXTHandler.LeaveWarning,
                OnYesAction = ProccessLeaveTournament
            });
        }

        public void FinishTournament()
        {
            Tournament.FinishTournament(onFinish => { 
                if (onFinish.IsSuccess)
                {
                    var finishPrefab = Prefabs.TournamentFinish;
                    var finishWindow = UIView.ShowWindow(finishPrefab);
                    finishWindow.GetComponent<TournamentResult>().Display(onFinish, ReDrawState);
                }
                else
                {
                    new PopupViewer().ShowStackError(onFinish.Error);
                }
            });
        }
    }
}

using CBS.Core;
using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class MatchmakingQueue : MonoBehaviour, IScrollableItem<CBSMatchmakingQueue>
    {
        [SerializeField]
        private Text TeamLabel;
        [SerializeField]
        private Text CountLabel;
        [SerializeField]
        private Text NameLabel;

        private IMatchmaking Matchmaking { get; set; }
        private CBSMatchmakingQueue Queue { get; set; }
        private MatchmakingPrefabs Prefabs { get; set; }

        private void Awake()
        {
            Matchmaking = CBSModule.Get<CBSMatchmaking>();
            Prefabs = CBSScriptable.Get<MatchmakingPrefabs>();
        }

        public void Display(CBSMatchmakingQueue queue)
        {
            Queue = queue;

            // draw ui
            CountLabel.text = Queue.PlayersCount.ToString();
            TeamLabel.text = Queue.Mode.ToString();
            NameLabel.text = Queue.QueueName;
        }

        public void StartMatchmaking()
        {
            var queueName = Queue.QueueName;

            Matchmaking.FindMatch(new FindMatchRequest
            {
                QueueName = queueName,
                StringEqualityValue = "val",
                DifferenceValue = Random.Range(0,5)
            },
            OnFindMatch);
        }

        // event 
        private void OnFindMatch(FindMatchResult result)
        {
            if (result.IsSuccess)
            {
                var queue = result.Queue;
                // hide matchmaking
                var matchPrefab = Prefabs.MatchmalingWindow;
                UIView.HideWindow(matchPrefab);
                // show wait windows
                var windowPrefab = Prefabs.WaitOpponentWindow;
                var waitWindow = UIView.ShowWindow(windowPrefab);
                waitWindow.GetComponent<WaitOpponent>().SetQueue(queue);
            }
            else
            {
                new PopupViewer().ShowFabError(result.Error);
            }
        }
    }
}

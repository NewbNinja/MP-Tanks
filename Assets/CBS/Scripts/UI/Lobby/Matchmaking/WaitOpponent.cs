using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class WaitOpponent : MonoBehaviour
    {
        private CBSMatchmakingQueue Queue { get; set; }

        private MatchmakingPrefabs Prefabs { get; set; }

        private IMatchmaking Matchmaking { get; set; }
        private IMatchmakingFactory MatchmakingFactory { get; set; }

        private void Awake()
        {
            Matchmaking = CBSModule.Get<CBSMatchmaking>();
            Prefabs = CBSScriptable.Get<MatchmakingPrefabs>();
            MatchmakingFactory = new MatchmakingFactory();

            Matchmaking.OnMatchStart += OnMatchStart;
            Matchmaking.OnStatusChanged += OnStatusChanged;
        }

        private void OnDestroy()
        {
            Matchmaking.OnMatchStart -= OnMatchStart;
            Matchmaking.OnStatusChanged -= OnStatusChanged;
        }

        private void OnStatusChanged(MatchmakingStatus status)
        {
            if (status == MatchmakingStatus.Canceled)
            {
                var matchmakingPrefab = Prefabs.MatchmalingWindow;
                UIView.ShowWindow(matchmakingPrefab);
                gameObject.SetActive(false);
            }
        }

        private void OnMatchStart(StartMatchResult result)
        {
            var previewPrefab = MatchmakingFactory.SpawnMatchmakingResult(Queue);
            previewPrefab.GetComponent<IMatchmakingPreview>().Draw(result);
            gameObject.SetActive(false);
        }

        public void SetQueue(CBSMatchmakingQueue queue)
        {
            Queue = queue;
        }

        // button clicks
        public void OnCancel()
        {
            var queueName = Queue.QueueName;
            if (string.IsNullOrEmpty(queueName))
                return;
            Matchmaking.CancelMatch(queueName, onCancel => {
                gameObject.SetActive(false);
            });
        }
    }
}

using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class MatchmakingWindow : MonoBehaviour
    {
        [SerializeField]
        private MatchmakingScroller Scroller;

        private IMatchmaking Matchmaking { get; set; }
        private MatchmakingPrefabs Prefabs { get; set; }

        private void Awake()
        {
            Matchmaking = CBSModule.Get<CBSMatchmaking>();
            Prefabs = CBSScriptable.Get<MatchmakingPrefabs>();
        }

        private void OnEnable()
        {
            Scroller.HideAll();
            Matchmaking.GetMatchmakingQueuesList(OnGetQueueList);
        }

        private void OnGetQueueList(GetMatchmakingListResult result)
        {
            if (result.IsSuccess)
            {
                var queuesList = result.Queues;
                var listPrefab = Prefabs.MatchmalingQueue;
                Scroller.Spawn(listPrefab, queuesList);
            }
            else
            {
                new PopupViewer().ShowFabError(result.Error);
            }
        }
    }
}

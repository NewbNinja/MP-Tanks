using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class DailyTasksWindow : MonoBehaviour
    {
        [SerializeField]
        private TasksScroller Scroller;

        private IDailyTasks DailyTasks { get; set; }
        private DailyTasksPrefabs Prefab { get; set; }

        private void Awake()
        {
            DailyTasks = CBSModule.Get<CBSDailyTasks>();
            Prefab = CBSScriptable.Get<DailyTasksPrefabs>();
        }

        private void OnEnable()
        {
            Scroller.HideAll();
            LoadAndShowDailyTasks();
        }

        private void LoadAndShowDailyTasks()
        {
            DailyTasks.GetPlayerDailyTasks(OnGetDailyTasks);
        }

        private void OnGetDailyTasks(GetPlayerDailyTasksResult result)
        {
            if (result.IsSuccess)
            {
                var tasks = result.CurrentTasks;
                DisplayTasks(tasks);
            }
            else
            {
                new PopupViewer().ShowFabError(result.Error);
                gameObject.SetActive(false);
            }
        }

        private void DisplayTasks(List<CBSTask> tasks)
        {
            var taskPrefab = Prefab.TaskSlot;
            Scroller.Spawn(taskPrefab, tasks);
        }

        // button click events
        public void ResetTasks()
        {
            Scroller.HideAll();
            DailyTasks.ResetAndGetNewTasks(onReset => {
                if (onReset.IsSuccess)
                {
                    var newTasks = onReset.CurrentTasks;
                    DisplayTasks(newTasks);
                }
                else
                {
                    Debug.Log(onReset.Error.Message);
                }
            });
        }
    }
}

using System;
using System.Collections.Generic;

namespace CBS
{
    [Serializable]
    public class PlayerTasksResponeData
    {
        public List<CBSTask> Tasks;
    }

    [Serializable]
    public class DailyTasksData : CBSTasksData<CBSTask>
    {
        public List<CBSTask> DailyTasks;
        public int DailyTasksCount;

        public override void Add(CBSTask task)
        {
            DailyTasks.Add(task);
        }

        public override List<CBSTask> GetTasks()
        {
            return DailyTasks;
        }

        public override List<CBSTask> NewInstance()
        {
            DailyTasks = new List<CBSTask>();
            return DailyTasks;
        }

        public override void Remove(CBSTask task)
        {
            if (DailyTasks.Contains(task))
            {
                DailyTasks.Remove(task);
                DailyTasks.TrimExcess();
            }
        }
    }
}
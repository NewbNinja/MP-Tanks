using System;
using System.Collections.Generic;

namespace CBS
{
    [Serializable]
    public class AchievementsData : CBSTasksData<CBSTask>
    {
        public List<CBSTask> Achievements;

        public override void Add(CBSTask task)
        {
            Achievements.Add(task);
        }

        public override List<CBSTask> GetTasks()
        {
            return Achievements;
        }

        public override List<CBSTask> NewInstance()
        {
            Achievements = new List<CBSTask>();
            return Achievements;
        }

        public override void Remove(CBSTask task)
        {
            if (Achievements.Contains(task))
            {
                Achievements.Remove(task);
                Achievements.TrimExcess();
            }
        }
    }

    public enum ModifyMethod
    {
        ADD = 0,
        UPDATE = 1
    }
}

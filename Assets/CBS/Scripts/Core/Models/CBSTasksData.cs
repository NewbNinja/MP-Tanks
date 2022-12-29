using System.Collections.Generic;

namespace CBS
{
    [System.Serializable]
    public abstract class CBSTasksData<T> where T : CBSTask
    {
        public bool AutomaticReward;

        public abstract List<T> GetTasks();


        public abstract void Add(T task);

        public abstract void Remove(T task);

        public abstract List<T> NewInstance();
    }
}

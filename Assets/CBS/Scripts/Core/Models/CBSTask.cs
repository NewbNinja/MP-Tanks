namespace CBS
{
    [System.Serializable]
    public class CBSTask
    {
        // static data
        public string ID;
        public string Title;
        public string Description;
        public string Tag;
        public TaskType Type;
        public int Steps;
        public bool IsLockedByLevel;
        public int LockLevel;
        public string ExternalUrl;
        public PrizeObject Reward;

        public BaseTaskState TaskState;

        // dynamic data
        public bool IsComplete => TaskState == null ? false : TaskState.IsComplete;
        public bool IsAvailable => TaskState == null ? true : TaskState.IsAvailable;
        public bool Rewarded => TaskState == null ? true : TaskState.Rewarded;
        public int CurrentStep => TaskState == null ? 0 : TaskState.CurrentStep;

        public void UpdateState(BaseTaskState state) => TaskState = state;
    }

    public enum TaskType
    {
        ONE_SHOT,
        STEPS
    }
}

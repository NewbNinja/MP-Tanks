namespace CBS
{
    public class BaseTaskState
    {
        public bool IsComplete;
        public int CurrentStep;
        public bool Rewarded;
        public bool IsAvailable = true;
    }
}
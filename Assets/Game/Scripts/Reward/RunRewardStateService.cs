namespace Game.Scripts.Reward
{
    public class RunRewardStateService
    {
        public bool ReviveUsed { get; private set; }
        public void MarkReviveUsed() => ReviveUsed = true;
        public void ResetRunFlags() => ReviveUsed = false;
    }
}
using UnityEngine;

namespace Game.Scripts.Reward
{
    [CreateAssetMenu(menuName = "Game/Rewards/RewardConfig")]
    public class RewardConfig : ScriptableObject
    {
        public string ReviveAdId = "revive";
        public float ReviveHealthPercent = 0.4f;
        public string WinBonusAdId = "win_bonus";
        public int WinBonusXp = 150;
        public string RerollAdId = "upgrade_reroll";
    }
}
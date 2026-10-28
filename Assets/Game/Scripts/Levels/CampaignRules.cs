using UnityEngine;

namespace Game.Scripts.Levels
{
    public static class CampaignRules
    {
        public const int StageCount = 20;
        public static (int index, int cycle) Next(int index, int cycle) =>
            index >= StageCount - 1 ? (2, cycle + 1) : (index + 1, cycle);
        public static float EnemyHealth(int cycle) => 1f + .35f * Mathf.Max(0, cycle);
        public static float EnemyDamage(int cycle) => 1f + .15f * Mathf.Max(0, cycle);
        public static float Experience(int cycle) => 1f + .25f * Mathf.Max(0, cycle);
        public static int Rerolls(int unlockedStage) => unlockedStage >= 16 ? 2 : unlockedStage >= 10 ? 1 : 0;
        public static int Score(int index, int cycle) => 18 * cycle + index + 1;
    }
}

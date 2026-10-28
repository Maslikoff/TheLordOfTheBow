using Game.Scripts.Upgrades;
using UnityEngine;

namespace Game.Scripts.UpgradeCard
{
    [CreateAssetMenu(menuName = "Game/Upgrades/Campaign")]
    public class CampaignUpgradeCard : Upgrades.UpgradeCard
    {
        [SerializeField] private CampaignUpgradeKind _kind;
        public CampaignUpgradeKind Kind => _kind;
        public string Id => _kind.ToString();
        public override void Apply(PlayerUpgradeHolder holder) => holder.Campaign.Apply(_kind);
        public bool IsAvailable(PlayerUpgradeHolder holder) => holder != null && holder.Campaign.CanApply(_kind);
        public string Describe(PlayerUpgradeHolder holder)
        {
            if (holder == null || _kind == CampaignUpgradeKind.Heal || _kind == CampaignUpgradeKind.Coins) return Description;
            int rank = holder.Campaign.EffectiveRank(_kind);
            string label = YG.YG2.lang switch { "en" => "Rank", "tr" => "Seviye", _ => "Ранг" };
            return $"{Description}\n{label}: {rank} → {rank + 1}\n{holder.Campaign.Preview(_kind)}";
        }
    }
}

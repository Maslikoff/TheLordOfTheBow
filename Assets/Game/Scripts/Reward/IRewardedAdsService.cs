using Cysharp.Threading.Tasks;

namespace Game.Scripts.Reward
{
    public interface IRewardedAdsService
    {
        bool IsBusy { get; }
        UniTask<RewardAdResult> ShowAsync(string rewardId);
    }
}
using Cysharp.Threading.Tasks;
using JTLStudio.SDK;

namespace Game.Scripts.Reward
{
    /// <summary>Rewarded ads via JTL SDK. A single callback replaces the three PluginYG2 events.</summary>
    public class YgRewardedAdsService : IRewardedAdsService
    {
        public bool IsBusy { get; private set; }

        public async UniTask<RewardAdResult> ShowAsync(string rewardId)
        {
            if (IsBusy)
                return RewardAdResult.Busy;

            if (JTLSDK.IsCreated == false || JTLSDK.Ads.IsRewardedSupported == false)
                return RewardAdResult.Error;

            IsBusy = true;
            var tcs = new UniTaskCompletionSource<RewardAdResult>();

            try
            {
                JTLSDK.Ads.ShowRewarded(rewardId, result =>
                {
                    switch (result)
                    {
                        case AdResult.Rewarded:
                            tcs.TrySetResult(RewardAdResult.Granted);
                            break;
                        case AdResult.Shown:
                        case AdResult.Closed:
                        case AdResult.NotShown:
                            tcs.TrySetResult(RewardAdResult.ClosedWithoutReward);
                            break;
                        default:
                            tcs.TrySetResult(RewardAdResult.Error);
                            break;
                    }
                });

                return await tcs.Task;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

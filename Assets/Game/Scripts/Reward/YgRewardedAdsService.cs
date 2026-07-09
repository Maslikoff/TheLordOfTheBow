using Cysharp.Threading.Tasks;
using YG;

namespace Game.Scripts.Reward
{
    public class YgRewardedAdsService : IRewardedAdsService
    {
        public bool IsBusy { get; private set; }

        public async UniTask<RewardAdResult> ShowAsync(string rewardId)
        {
            if (IsBusy)
                return RewardAdResult.Busy;

            if (YG2.isSDKEnabled == false)
                return RewardAdResult.Error;

            IsBusy = true;
            var tcs = new UniTaskCompletionSource<RewardAdResult>();
            bool rewarded = false;

            void OnReward(string id)
            {
                if (string.IsNullOrEmpty(rewardId) || id == rewardId)
                    rewarded = true;
            }

            void OnError()
            {
                tcs.TrySetResult(RewardAdResult.Error);
            }

            void OnClose()
            {
                tcs.TrySetResult(
                    rewarded ? RewardAdResult.Granted : RewardAdResult.ClosedWithoutReward);
            }

            YG2.onRewardAdv += OnReward;
            YG2.onErrorRewardedAdv += OnError;
            YG2.onCloseRewardedAdv += OnClose;

            try
            {
                YG2.RewardedAdvShow(rewardId);
                return await tcs.Task;
            }
            finally
            {
                YG2.onRewardAdv -= OnReward;
                YG2.onErrorRewardedAdv -= OnError;
                YG2.onCloseRewardedAdv -= OnClose;
                IsBusy = false;
            }
        }
    }
}
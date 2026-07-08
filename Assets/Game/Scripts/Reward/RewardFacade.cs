using Cysharp.Threading.Tasks;
using Game.Scripts.Characters.Player;
    
namespace Game.Scripts.Reward
{

    public class RewardFacade
    {
        private readonly IRewardedAdsService _ads;
        private readonly RewardConfig _config;
        private readonly RunRewardStateService _runState;

        public RewardFacade(IRewardedAdsService ads, RewardConfig config, RunRewardStateService runState)
        {
            _ads = ads;
            _config = config;
            _runState = runState;
        }

        public async UniTask<bool> TryReviveAsync(Player player)
        {
            if (player == null || _runState.ReviveUsed) return false;

            var result = await _ads.ShowAsync(_config.ReviveAdId);
            if (result != RewardAdResult.Granted) return false;

            bool revived = player.TryRevive(_config.ReviveHealthPercent);
            if (revived) _runState.MarkReviveUsed();
            return revived;
        }

        public async UniTask<bool> TryWinBonusAsync(Experience.Experience exp)
        {
            if (exp == null) return false;

            var result = await _ads.ShowAsync(_config.WinBonusAdId);
            if (result != RewardAdResult.Granted) return false;

            exp.AddExperience(_config.WinBonusXp);
            return true;
        }

        public async UniTask<bool> TryRerollAsync(System.Action rerollAction)
        {
            if (rerollAction == null) return false;

            var result = await _ads.ShowAsync(_config.RerollAdId);
            if (result != RewardAdResult.Granted) return false;

            rerollAction.Invoke();
            return true;
        }
    }
}
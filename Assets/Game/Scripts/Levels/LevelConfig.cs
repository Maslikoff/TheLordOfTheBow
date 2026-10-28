using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Scripts.AddressablesContent;
using Game.Scripts.Characters.Enemy;
using Game.Scripts.Wave;
using UnityEngine;

namespace Game.Scripts.Levels
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/Levels/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [SerializeField] private SceneNames _sceneNames;
        [SerializeField] private string _stageName;
        public string StageName => _stageName;
        [SerializeField] private SceneTransitionMode _sceneTransitionMode;
        [SerializeField] private bool _onGameStarted;
        [SerializeField] private WaveConfig _waveConfig;
        [SerializeField] private List<EnemyRaceConfig> _enemyRaceConfigs = new();
        [SerializeField] private List<Upgrades.UpgradeCard> _availableUpgrades = new();

        public SceneNames SceneNames => _sceneNames;
        public WaveConfig WaveConfig => _waveConfig;
        public IReadOnlyList<EnemyRaceConfig> EnemyRaceConfigs => _enemyRaceConfigs;
        public IReadOnlyList<Upgrades.UpgradeCard> AvailableUpgrades => _availableUpgrades;

        public async UniTask ResolveDependenciesAsync(IAddressablesContentService content)
        {
            if (_enemyRaceConfigs != null)
            {
                foreach (EnemyRaceConfig config in _enemyRaceConfigs)
                {
                    if (config != null)
                        await config.ResolveAsync(content);
                }
            }

            if (_availableUpgrades == null)
                return;

            foreach (Upgrades.UpgradeCard upgrade in _availableUpgrades)
            {
                if (upgrade != null)
                    await upgrade.EnsureIconLoadedAsync(content);
            }
        }
    }
}

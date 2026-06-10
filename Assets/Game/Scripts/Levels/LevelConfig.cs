using System.Collections.Generic;
using Game.Scripts.Characters.Enemy;
using Game.Scripts.Wave;
using UnityEngine;

namespace Game.Scripts.Levels
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/Levels/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [SerializeField] private SceneNames _sceneNames;
        [SerializeField] private SceneTransitionMode _sceneTransitionMode;
        [SerializeField] private bool _onGameStarted;
        [SerializeField] private WaveConfig _waveConfig;
        [SerializeField] private List<EnemyRaceConfig> _enemyRaceConfigs = new();
        [SerializeField] private List<Upgrades.UpgradeCard> _availableUpgrades = new();
        
        public SceneNames SceneNames => _sceneNames;
        public WaveConfig WaveConfig => _waveConfig;
        public IReadOnlyList<EnemyRaceConfig> EnemyRaceConfigs => _enemyRaceConfigs;
        public IReadOnlyList<Upgrades.UpgradeCard> AvailableUpgrades => _availableUpgrades;
    }
}
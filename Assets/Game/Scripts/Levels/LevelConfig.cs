using Game.Scripts.Wave;
using UnityEngine;

namespace Game.Scripts.Levels
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/Levels/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [SerializeField] private SceneNames _sceneNames;
        [SerializeField] private WaveConfig _waveConfig;
        
        public SceneNames SceneNames => _sceneNames;
        public WaveConfig WaveConfig => _waveConfig;
    }
}
using Game.Scripts.Characters.Player;
using UnityEngine;

namespace Game.Scripts.Levels
{
    [CreateAssetMenu(fileName = "GameStartupConfig", menuName = "Game/Startup Config")]
    public class GameStartupConfig : ScriptableObject
    {
        [SerializeField] private Player _playerPrefab;
        
        public Player PlayerPrefab => _playerPrefab;
    }
}
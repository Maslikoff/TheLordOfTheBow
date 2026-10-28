using Game.Scripts.AddressablesContent;
using Game.Scripts.Characters.Player;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Scripts.Levels
{
    [CreateAssetMenu(fileName = "GameStartupConfig", menuName = "Game/Startup Config")]
    public class GameStartupConfig : ScriptableObject
    {
        [SerializeField] private AssetReferenceGameObject _playerPrefabReference;

        private Player _resolvedPlayerPrefab;

        public AssetReferenceGameObject PlayerPrefabReference => _playerPrefabReference;

        public async UniTask<Player> GetPlayerPrefabAsync(IAddressablesContentService content)
        {
            if (_resolvedPlayerPrefab != null)
                return _resolvedPlayerPrefab;

            GameObject go = await content.LoadGameObjectAsync(_playerPrefabReference);
            _resolvedPlayerPrefab = go.GetComponent<Player>();

            if (_resolvedPlayerPrefab == null)
                throw new MissingComponentException("Player prefab has no Player component.");

            return _resolvedPlayerPrefab;
        }
    }
}

using UnityEngine;

namespace Assets.Game.Scripts.Characters.Player
{
    public class PlayerTransformHolder : MonoBehaviour, ITransformHolder
    {
        [SerializeField] private Transform _playerTransform;

        public Transform Transform => _playerTransform;
    }
}
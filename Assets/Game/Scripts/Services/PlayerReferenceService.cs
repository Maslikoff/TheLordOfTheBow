using UnityEngine;

namespace Game.Scripts.Services
{
    public class PlayerReferenceService
    {
        public Transform PlayerTransform { get; private set; }

        public void SetPlayerTransform(Transform playerTransform)
        {
            PlayerTransform = playerTransform;
        }
    }
}
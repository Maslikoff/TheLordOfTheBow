using UnityEngine;

namespace Game.Scripts.Levels
{
    public class PlayerSpawnPoint : MonoBehaviour
    {
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;
    }
}
using Game.Scripts.Characters.Bullets;
using UnityEngine;

namespace Game.Scripts.Environment
{
    public class DetectZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out Ground ground))
                ground.Release();
            
            if (other.gameObject.TryGetComponent(out Bullet bullet))
                bullet.Release();
        }
    }
}
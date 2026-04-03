using System;
using Game.Scripts.Characters.Bullets;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    [Serializable]
    public class BulletUIConfig
    {
        [SerializeField] private BulletType _bulletType;
        [SerializeField] private Cell _cellObject;
        [SerializeField] private Image _cooldownImage;
        
        public BulletType BulletType => _bulletType;
        public Cell CellObject => _cellObject;
        public Image CooldownImage => _cooldownImage;
    }
}
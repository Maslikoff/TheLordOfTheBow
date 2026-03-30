using UnityEngine;

namespace Game.Scripts.Upgrades
{
     [CreateAssetMenu(fileName = "New Upgrade Card", menuName = "Game/Upgrade Card")]
    public class UpgradeCard : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string _cardName;
        [SerializeField] private string _description;
        [SerializeField] private Sprite _cardIcon;
        [SerializeField] private UpgradeType _upgradeType;
        
        [Header("Upgrade Values")]
        [SerializeField] private float _value;
        
        [Header("Visual")]
        [SerializeField] private Color _cardColor = Color.white;
        
        public string CardName => _cardName;
        public string Description => _description;
        public Sprite CardIcon => _cardIcon;
        public UpgradeType UpgradeType => _upgradeType;
        public float Value => _value;
        public Color CardColor => _cardColor;
    }
}
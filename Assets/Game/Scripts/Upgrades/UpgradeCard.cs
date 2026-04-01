using UnityEngine;

namespace Game.Scripts.Upgrades
{
    public abstract class UpgradeCard : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string _cardName;
        [SerializeField] private string _description;
        [SerializeField] private Sprite _cardIcon;
        
        [Header("Visual")]
        [SerializeField] private Color _cardColor = Color.white;
        
        public string CardName => _cardName;
        public string Description => _description;
        public Sprite CardIcon => _cardIcon;
        public Color CardColor => _cardColor;

        public abstract void Apply(PlayerUpgradeHolder playerUpgradeHolder);
    }
}
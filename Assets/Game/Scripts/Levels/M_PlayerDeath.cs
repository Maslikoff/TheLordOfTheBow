using Game.Scripts.Characters.Player;

namespace Game.Scripts.Levels
{
    public struct M_PlayerDeath
    {
        public Player Player { get; }
    
        public M_PlayerDeath(Player player)
        {
            Player = player;
        }
    }
}
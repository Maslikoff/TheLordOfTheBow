using Game.Scripts.Characters.Player;

namespace Game.Scripts.Levels
{
    public struct M_PlayerSpawned
    {
        public Player Player { get; }
        
        public M_PlayerSpawned(Player player)
        {
            Player = player;
        }
    }
}
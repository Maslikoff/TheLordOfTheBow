namespace Game.Scripts.Characters.Player
{
    public interface IPlayerProvider
    {
        Player CurrentPlayer { get; }
        Player Player { get; }
    }
}
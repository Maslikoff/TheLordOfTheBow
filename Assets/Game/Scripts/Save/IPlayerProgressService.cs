using Game.Scripts.Characters.Player;
using YG;

namespace Game.Scripts.Save
{
    public interface IPlayerProgressService
    {
        bool HasSessionProgress { get; }
        void CaptureFrom(Player player);
        void ApplyTo(Player player);
        void SyncToSaves(SavesYG saves);
        void LoadFromSaves(SavesYG saves);
    }
}

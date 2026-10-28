using Game.Scripts.Save;

namespace Game.Scripts.Levels
{
    public class LevelSessionService
    {
        public PlayerProgressSnapshot Entry { get; set; }
        public PlayerProgressSnapshot Wave { get; set; }
        public PlayerProgressSnapshot PreDeath { get; set; }
        public int WaveIndex { get; set; }
        public void Reset()
        {
            Entry = Wave = PreDeath = null;
            WaveIndex = 0;
        }
    }
}

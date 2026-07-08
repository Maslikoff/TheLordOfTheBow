using System;

namespace Game.Scripts.Save
{
    [Serializable]
    public class LevelWaveCheckpoint
    {
        public int LevelIndex;
        public int WaveIndex;
    }
}
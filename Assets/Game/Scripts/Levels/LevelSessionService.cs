using UnityEngine;

namespace Game.Scripts.Levels
{
    public class LevelSessionService
    {
        public int SnapshotLevel { get; private set; }
        public float SnapshotExperience { get; private set; }
        public bool HasSnapshot { get; private set; }

        public int PreDeathLevel { get; private set; }
        public float PreDeathExperience { get; private set; }
        public bool HasPreDeathState { get; private set; }

        public void CaptureSnapshot(Experience.Experience experience)
        {
            if (experience == null)
                return;

            SnapshotLevel = experience.CurrentLevel;
            SnapshotExperience = experience.CurrentExperience;
            HasSnapshot = true;
            ClearPreDeathState();

            Debug.Log($"[LevelSession] Снимок входа: Lvl {SnapshotLevel}, XP {SnapshotExperience}");
        }

        public void CapturePreDeathState(Experience.Experience experience)
        {
            if (experience == null)
                return;

            PreDeathLevel = experience.CurrentLevel;
            PreDeathExperience = experience.CurrentExperience;
            HasPreDeathState = true;

            Debug.Log($"[LevelSession] XP до смерти: Lvl {PreDeathLevel}, XP {PreDeathExperience}");
        }

        public void RollbackExperience(Experience.Experience experience)
        {
            if (experience == null || HasSnapshot == false)
                return;

            experience.LoadSaveData(SnapshotLevel, SnapshotExperience);

            Debug.Log($"[LevelSession] Откат к входу: Lvl {SnapshotLevel}, XP {SnapshotExperience}");
        }

        public void RestorePreDeathExperience(Experience.Experience experience)
        {
            if (experience == null || HasPreDeathState == false)
                return;

            experience.LoadSaveData(PreDeathLevel, PreDeathExperience);

            Debug.Log($"[LevelSession] Восстановлен XP до смерти: Lvl {PreDeathLevel}, XP {PreDeathExperience}");
        }

        public void ClearPreDeathState()
        {
            HasPreDeathState = false;
            PreDeathLevel = 0;
            PreDeathExperience = 0f;
        }
    }
}
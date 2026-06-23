namespace Game.Scripts.Save
{
    public interface ISaveSystem
    {
        void SaveGameData();
        void LoadGameData();
        void ManualSave();
    }
}
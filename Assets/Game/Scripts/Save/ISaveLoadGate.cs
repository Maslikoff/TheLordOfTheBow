using Cysharp.Threading.Tasks;

namespace Game.Scripts.Save
{
    public interface ISaveLoadGate
    {
        bool IsReady { get; }
        UniTask WaitUntilReadyAsync();
    }
}
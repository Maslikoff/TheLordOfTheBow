using System;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Game.Scripts.Levels
{
    public class Bootstrapper : IStartable
    {
        private readonly ILevelService _levelService;
        
        public Bootstrapper(ILevelService levelService)
        {
            _levelService = levelService ?? throw new ArgumentNullException(nameof(levelService));
        }
        
        public void Start()
        {
            _levelService.LoadCurrentLevelAsync().Forget();
        }
    }
}
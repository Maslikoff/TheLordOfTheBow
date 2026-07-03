using System;
using Cysharp.Threading.Tasks;
using Game.Scripts.Save;
using UnityEngine;
using VContainer.Unity;

namespace Game.Scripts.Levels
{
    public class Bootstrapper : IStartable
    {
        private readonly ILevelService _levelService;
        private readonly ISaveLoadGate _saveLoadGate;
        
        public Bootstrapper(ILevelService levelService, ISaveLoadGate saveLoadGate)
        {
            _levelService = levelService ?? throw new ArgumentNullException(nameof(levelService));
            _saveLoadGate = saveLoadGate ?? throw new ArgumentNullException(nameof(saveLoadGate));
        }
        
        public void Start()
        {
            StartAsync().Forget();
        }
        
        private async UniTaskVoid StartAsync()
        {
            Debug.Log("[Bootstrapper] Ожидание облачных сохранений...");
            await _saveLoadGate.WaitUntilReadyAsync();
            
            Debug.Log("[Bootstrapper] Сохранения готовы — загрузка уровня");
            await _levelService.LoadCurrentLevelAsync();
        }
    }
}
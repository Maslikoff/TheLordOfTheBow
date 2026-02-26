using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.PoolSystem
{
    public class PooledObjectInjector : MonoBehaviour
    {
        private IObjectResolver _container;
        
        [Inject]
        public void Construct(IObjectResolver container)
        {
            _container = container;
        }

        public void InjectGameObject(GameObject gameObject)
        {
            if (_container == null)
                return;

            var monoBehaviours = gameObject.GetComponents<MonoBehaviour>();
            
            foreach (var mb in monoBehaviours)
                if (mb != null)
                    _container.Inject(mb);
        }
    }
}
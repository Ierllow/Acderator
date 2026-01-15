using UnityEngine;
using Zenject;

namespace Intense
{
    public class GameManagerInstaller : MonoInstaller<GameManagerInstaller>
    {
        [SerializeField] private SceneManager sceneManager;

        public override void InstallBindings() => Container.Bind<SceneManager>().FromInstance(sceneManager).AsSingle().NonLazy();
    }
}
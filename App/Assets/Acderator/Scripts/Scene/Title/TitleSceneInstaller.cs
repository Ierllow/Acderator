using UnityEngine;
using Zenject;

namespace Title
{
    public class TitleSceneInstaller : MonoInstaller<TitleSceneInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<TitleAuthController>().AsSingle();
        }
    }
}
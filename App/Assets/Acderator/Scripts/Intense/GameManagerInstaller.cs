using UnityEngine;
using Zenject;
using Intense.Api;
using Intense.Asset;
using Intense.Data;
using Intense.Master;
using Intense.UI;

namespace Intense
{
    public class GameManagerInstaller : MonoInstaller<GameManagerInstaller>
    {
        [SerializeField] private SceneManager sceneManager;

        public override void InstallBindings()
        {
            Container.Bind<SceneManager>().FromInstance(sceneManager).AsSingle().NonLazy();
            Container.Bind<NetworkManager>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<MasterDataManager>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<ScoreManager>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<SoundManager>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<AssetBundleManager>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<PopupManager>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<Loading>().FromComponentInHierarchy().AsSingle().NonLazy();

            Container.Inject(typeof(PopupUtils));
            Container.Inject(typeof(PopupContextFactory));
        }
    }
}

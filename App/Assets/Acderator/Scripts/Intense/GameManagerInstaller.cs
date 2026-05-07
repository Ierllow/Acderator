using Intense.Api;
using Intense.Asset;
using Intense.Data;
using Intense.Master;
using Intense.UI;
using UnityEngine;
using Zenject;

namespace Intense
{
    public class GameManagerInstaller : MonoInstaller<GameManagerInstaller>
    {
        [SerializeField] private SceneManager sceneManager;
        [SerializeField] private Loading loading;
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private SoundManager soundManager;
        [SerializeField] private AssetBundleManager assetBundleManager;
        [SerializeField] private PopupManager popupManager;

        public override void InstallBindings()
        {
            Container.Bind<SceneManager>().FromInstance(sceneManager).AsSingle().NonLazy();
            Container.Bind<Loading>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<IApiSession>().To<ApiSession>().AsSingle().NonLazy();
            Container.Bind<NetworkManager>().FromInstance(networkManager).AsSingle().NonLazy();
            Container.Bind<MasterDataManager>().AsSingle().NonLazy();
            Container.Bind<ScoreManager>().AsSingle().NonLazy();
            Container.Bind<SoundManager>().AsSingle().NonLazy();
            Container.Bind<AssetBundleManager>().FromInstance(assetBundleManager).AsSingle().NonLazy();
            Container.Bind<PopupManager>().FromInstance(popupManager).AsSingle().NonLazy();
            Container.Bind<SceneErrorPopupController>().AsSingle();
            Container.Bind<SceneErrorHandler>().AsTransient();
        }
    }
}
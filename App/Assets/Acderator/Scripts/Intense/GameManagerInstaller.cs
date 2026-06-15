using Intense.Api;
using Intense.Asset;
using Intense.Data;
using Intense.Master;
using Intense.UI;
using Song;
using Zenject;

namespace Intense
{
    public class GameManagerInstaller : MonoInstaller<GameManagerInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<SceneManager>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<Loading>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<NetworkManager>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<AddressableAssetCache>().AsSingle();
            Container.BindInterfacesAndSelfTo<AddressableAssetManager>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<IApiSession>().To<ApiSession>().AsSingle().NonLazy();
            Container.Bind<MasterDataManager>().AsSingle().NonLazy();
            Container.Bind<ScoreManager>().AsSingle().NonLazy();
            Container.Bind<CriAtomCueSheetLoader>().AsSingle().NonLazy();
            Container.Bind<SoundSheetNameResolver>().AsSingle().NonLazy();
            Container.Bind<SongSoundSheetNameResolver>().AsSingle().NonLazy();
            Container.Bind<BgmVolumeController>().AsSingle().NonLazy();
            Container.Bind<SeVolumeController>().AsSingle().NonLazy();
            Container.Bind<SongVolumeController>().AsSingle().NonLazy();
            Container.Bind<SoundManager>().AsSingle().NonLazy();
            Container.Bind<PopupManager>().AsSingle().NonLazy();
            Container.Bind<SceneErrorPopupController>().AsSingle();
            Container.Bind<SceneErrorHandler>().AsTransient();
            Container.Bind<AddressableAssetPopupController>().AsTransient();
        }
    }
}
using Cysharp.Threading.Tasks;
using Intense.Asset;
using Zenject;

namespace Intense
{
    public enum EBgmType { Stop = -1, None, GameResult, GameResultFailed }
    public enum ESeType { Tap, Flick }

    public class SoundManager : IInitializable, ISceneLoadedHandler
    {
        [Inject] private readonly CriAtomCueSheetLoader cueSheetLoader;
        [Inject] private readonly AddressableAssetManager addressableAssetManager;
        [Inject] private readonly SoundSheetNameResolver soundSheetNameResolver;
        [Inject] private readonly BgmVolumeController bgmVolumeController;
        [Inject] private readonly SeVolumeController seVolumeController;

        private CriAtomSoundPlayer bgmPlayer;
        private CriAtomSoundPlayer sePlayer;

        public void OnSceneLoaded(SceneContext context) => UpdateSounds(context.BgmType);

        private void UpdateSounds(EBgmType bgmType)
        {
            switch (bgmType)
            {
                case EBgmType.None:
                    break;
                case EBgmType.Stop:
                    bgmPlayer.Stop();
                    break;
                default:
                    bgmPlayer.Stop();
                    PlayBgm(bgmType, true);
                    break;
            }
        }

        public void Initialize()
        {
            bgmPlayer = new();
            sePlayer = new();
            bgmVolumeController.Register(bgmPlayer);
            seVolumeController.Register(sePlayer);
        }

        public void PlaySe(ESeType type) => UniTask.Void(async () =>
        {
            var mSoundCueName = soundSheetNameResolver.FindSe(type);
            var sheet = await cueSheetLoader.GetOrAddCueSheetAsync(mSoundCueName.SheetName, addressableAssetManager.Config.SongSe);
            sePlayer.Play(sheet, mSoundCueName.CueName);
        });

        public void PlayBgm(EBgmType type, bool isLoop = true) => UniTask.Void(async () =>
        {
            var mSoundCueName = soundSheetNameResolver.FindBgm(type);
            var sheet = await cueSheetLoader.GetOrAddCueSheetAsync(mSoundCueName.SheetName, addressableAssetManager.Config.Bgm);
            bgmPlayer.Play(sheet, mSoundCueName.CueName, isLoop);
        });
    }
}
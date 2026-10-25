using Cysharp.Threading.Tasks;
using Intense.Asset;
using Zenject;

namespace Intense
{
    public enum BgmType { Stop = -1, None, GameResult, GameResultFailed }
    public enum SeType { Tap, Flick }

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

        private void UpdateSounds(BgmType bgmType)
        {
            switch (bgmType)
            {
                case BgmType.None:
                    break;
                case BgmType.Stop:
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

        public void PlaySe(SeType type) => UniTask.Void(async () =>
        {
            var soundCue = soundSheetNameResolver.FindSe(type);
            var sheet = await cueSheetLoader.GetOrAddCueSheetAsync(soundCue.SheetName, addressableAssetManager.Config.SongSe);
            sePlayer.Play(sheet, soundCue.CueName);
        });

        public void PlayBgm(BgmType type, bool isLoop = true) => UniTask.Void(async () =>
        {
            var soundCue = soundSheetNameResolver.FindBgm(type);
            var sheet = await cueSheetLoader.GetOrAddCueSheetAsync(soundCue.SheetName, addressableAssetManager.Config.Bgm);
            bgmPlayer.Play(sheet, soundCue.CueName, isLoop);
        });
    }
}
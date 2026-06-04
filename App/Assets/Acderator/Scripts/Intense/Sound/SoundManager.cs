using Cysharp.Threading.Tasks;
using Zenject;

namespace Intense
{
    public enum EBgmType { Stop = -1, None, GameResult, GameResultFailed }
    public enum ESeType { Tap, Flick }

    public class SoundManager : IInitializable
    {
        [Inject] private readonly CriAtomCueSheetLoader cueSheetLoader;
        [Inject] private readonly SoundSheetNameResolver soundSheetNameResolver;
        [Inject] private readonly BgmVolumeController bgmVolumeController;
        [Inject] private readonly SeVolumeController seVolumeController;

        private readonly CriAtomSoundPlayer bgmPlayer = new();
        private readonly CriAtomSoundPlayer sePlayer = new();

        public void UpdateSounds(EBgmType bgmType)
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
            bgmVolumeController.Register(bgmPlayer);
            seVolumeController.Register(sePlayer);
        }

        public void PlaySe(ESeType type) => UniTask.Void(async () =>
        {
            var mSoundCueName = soundSheetNameResolver.FindSe(type);
            var sheet = await cueSheetLoader.GetOrAddCueSheetAsync(mSoundCueName.SheetName, "sounds/song/songse");
            sePlayer.Play(sheet, mSoundCueName.CueName);
        });

        public void PlayBgm(EBgmType type, bool isLoop = true) => UniTask.Void(async () =>
        {
            var mSoundCueName = soundSheetNameResolver.FindBgm(type);
            var sheet = await cueSheetLoader.GetOrAddCueSheetAsync(mSoundCueName.SheetName, "sounds/bgm/bgm");
            bgmPlayer.Play(sheet, mSoundCueName.CueName, isLoop);
        });
    }
}
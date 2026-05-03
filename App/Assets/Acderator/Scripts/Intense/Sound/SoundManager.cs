using CriWare;
using Cysharp.Threading.Tasks;
using Intense.Asset;
using Intense.Master;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace Intense
{
    public enum ESoundCategory { Bgm, Song, Preview, Se }
    public enum EBgmType { Stop = -1, None, Title, GameResult, GameResultFailed }
    public enum ESeType { Tap, Flick }

    public class SoundManager : IInitializable
    {
        [Inject] private AssetBundleManager assetBundleManager;
        [Inject] private MasterDataManager masterDataManager;

        public CriAtomExPlayer BgmExPlayer { get; private set; }
        public CriAtomExPlayer SongExPlayer { get; private set; }
        public CriAtomExPlayer SongPreviewExPlayer { get; private set; }
        public CriAtomExPlayer SeExPlayer { get; private set; }

        public void UpdateSounds(EBgmType bgmType)
        {
            switch (bgmType)
            {
                case EBgmType.None:
                    break;
                case EBgmType.Stop:
                    StopSoundAll();
                    break;
                default:
                    StopSoundAll();
                    PlayBgm(bgmType, true);
                    break;
            }
        }

        public void UpdateBgmVolume(float volume, bool isMute = false)
        {
            BgmExPlayer?.SetVolume(!isMute ? volume : 0f);
            BgmExPlayer?.UpdateAll();
        }

        public void UpdateSeVolume(float volume, bool isMute = false)
        {
            SeExPlayer?.SetVolume(!isMute ? volume : 0f);
            SeExPlayer?.UpdateAll();
        }

        public void UpdateSongVolume(float volume, bool isMute = false)
        {
            SongExPlayer?.SetVolume(!isMute ? volume : 0f);
            SongPreviewExPlayer?.SetVolume(!isMute ? volume : 0f);
            SongExPlayer?.UpdateAll();
            SongPreviewExPlayer?.UpdateAll();
        }

        public void Initialize()
        {
            BgmExPlayer = new CriAtomExPlayer();
            SongExPlayer = new CriAtomExPlayer();
            SongPreviewExPlayer = new CriAtomExPlayer();
            SeExPlayer = new CriAtomExPlayer();
            UpdateBgmVolume(PlayerPrefsValues.BV, PlayerPrefsValues.BM);
            UpdateSeVolume(PlayerPrefsValues.SV, PlayerPrefsValues.SM);
            UpdateSongVolume(PlayerPrefsValues.SGV, PlayerPrefsValues.SOM);
        }

        public async UniTask<CriAtomCueSheet> AddCueSheetAsync(string name, TextAsset asset)
        {
            var cueSheet = CriAtom.AddCueSheet(name, asset.bytes, "");
            await UniTask.WaitWhile(() => cueSheet.IsLoading);

            return cueSheet;
        }

        public async UniTask<CriAtomCueSheet> GetOrAddCueSheetAsync(string name, string sheetPath)
        {
            var sheet = CriAtom.GetCueSheet(name);
            var asset = await assetBundleManager.GetLoadedObjectAsync(sheetPath) as TextAsset;
            sheet ??= await AddCueSheetAsync(name, asset);
            if (sheet == default)
            {
                Debug.LogWarning(string.Format("{0} dose not exist", name));
                return default;
            }
            return sheet;
        }

        public void PlaySe(ESeType type) => UniTask.Void(async () =>
        {
            var mSoundCueName = masterDataManager.MemoryDatabase.SoundSheetNameMasterTable.First(x => x.Category == (int)ESoundCategory.Se && x.Id == (int)type);
            var sheet = await GetOrAddCueSheetAsync(mSoundCueName.SheetName, "sounds/song/songse");

            StopSe();
            SeExPlayer.SetCue(sheet.acb, mSoundCueName.CueName);
            SeExPlayer.Loop(false);
            SeExPlayer.Start();
        });

        public void PlayBgm(EBgmType type, bool isLoop = true) => UniTask.Void(async () =>
        {
            var mSoundCueName = masterDataManager.MemoryDatabase.SoundSheetNameMasterTable.First(x => x.Category == (int)ESoundCategory.Bgm && x.Id == (int)type);
            var sheet = await GetOrAddCueSheetAsync(mSoundCueName.SheetName, "sounds/bgm/bgm");

            StopBgm();
            BgmExPlayer.SetCue(sheet.acb, mSoundCueName.CueName);
            BgmExPlayer.Loop(isLoop);
            BgmExPlayer.Start();
        });

        public void PlaySong(int id) => UniTask.Void(async () =>
        {
            var mSoundCueName = masterDataManager.MemoryDatabase.SoundSheetNameMasterTable.First(x => x.Category == (int)ESoundCategory.Song);
            var sheet = await GetOrAddCueSheetAsync(mSoundCueName.SheetName, string.Format("sounds/song/song_{0}", id));

            StopSong();
            SongExPlayer.SetCue(sheet.acb, id.ToString());
            SongExPlayer.Loop(false);
            SongExPlayer.Start();
        });

        public void PlaySongPreview(int id, CancellationToken token = default) => UniTask.Void(async () =>
        {
            var mSoundCueName = masterDataManager.MemoryDatabase.SoundSheetNameMasterTable.First(x => x.Category == (int)ESoundCategory.Song);
            var sheet = await GetOrAddCueSheetAsync(mSoundCueName.SheetName, string.Format("sounds/song/song_{0}", id));

            SongPreviewExPlayer.AttachFader();
            SongPreviewExPlayer.SetFadeInTime(3000);
            SongPreviewExPlayer.SetFadeOutTime(3000);
            SongPreviewExPlayer.SetCue(sheet.acb, id.ToString());
            SongPreviewExPlayer.SetStartTime(masterDataManager.MemoryDatabase.SongSelectMasterTable.FindByGroup(id).StartSongTime);
            SongPreviewExPlayer.Start();
            var songSelectMaster = masterDataManager.MemoryDatabase.SongSelectMasterTable.FindByGroup(id);
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await UniTask.WaitUntil(() => SongPreviewExPlayer.GetTime() > songSelectMaster.StartSongTime + songSelectMaster.SongTime, cancellationToken: token);
                    await UniTask.WaitWhile(() => SongPreviewExPlayer.IsFading(), cancellationToken: token);
                    SongPreviewExPlayer.SetFadeInStartOffset(6000);
                    SongPreviewExPlayer.Start();
                }
            }
            catch (OperationCanceledException)
            {
                StopSongPreview();
            }
        });

        public void PauseSong(bool isPause) => SongExPlayer?.Pause(isPause);

        public void StopSe() => SeExPlayer?.Stop(true);

        public void StopBgm() => BgmExPlayer?.Stop(true);

        public void StopSong() => SongExPlayer?.Stop(true);

        public void StopSongPreview() => SongPreviewExPlayer?.Stop(true);

        public void StopSoundAll()
        {
            StopBgm();
            StopSong();
            StopSongPreview();
        }
    }
}

using CriWare;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Intense.Asset;
using Intense.Data;
using Intense.Master;
using System;
using System.Threading;
using UnityEngine;

namespace Intense
{
    public enum ESoundCategory { Bgm, Song, Preview, Se }
    public enum EBgmType { Stop = -1, None, Title, GameResult, GameResultFailed }
    public enum ESeType { Tap, Flick }

    public class SoundManager : SingletonMonoBehaviour<SoundManager>
    {
        public CriAtomExPlayer BgmExPlayer { get; private set; }
        public CriAtomExPlayer SongExPlayer { get; private set; }
        public CriAtomExPlayer SongPreviewExPlayer { get; private set; }
        public CriAtomExPlayer SeExPlayer { get; private set; }

        public bool IsInit { get; private set; } = false;

        protected override void OnDestroy()
        {
            StopBgm();
            BgmExPlayer?.Dispose();
            StopSong();
            SongExPlayer?.Dispose();
            StopSongPreview();
            SongPreviewExPlayer?.Dispose();
            StopSe();
            SeExPlayer?.Dispose();

            base.OnDestroy();
        }

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

        public async UniTask InitializeAsync()
        {
            if (IsInit) return;

            CriAtomEx.UnregisterAcf();

            BgmExPlayer ??= new CriAtomExPlayer();
            SongExPlayer ??= new CriAtomExPlayer();
            SongPreviewExPlayer ??= new CriAtomExPlayer();
            SeExPlayer ??= new CriAtomExPlayer();

            await UniTask.WaitUntil(() => LocalDataManager.Instance.Option.IsInit);
            UpdateBgmVolume(LocalDataManager.Instance.Option.BgmVolume, LocalDataManager.Instance.Option.IsMuteBgm);
            UpdateSeVolume(LocalDataManager.Instance.Option.SeVolume, LocalDataManager.Instance.Option.IsMuteSe);
            UpdateSongVolume(LocalDataManager.Instance.Option.SongVolume, LocalDataManager.Instance.Option.IsMuteSong);

            IsInit = true;
            await UniTask.WaitUntil(() => IsInit);
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
            var asset = await AssetBundleManager.Instance.GetLoadedObjectAsync(sheetPath) as TextAsset;
            sheet ??= await AddCueSheetAsync(name, asset);
            if (sheet == default)
            {
                Debug.LogWarning(ZString.Format("{0} dose not exist", name));
                return default;
            }
            return sheet;
        }

        public void PlaySe(ESeType type)
        {
            if (!IsInit) return;

            UniTask.Void(async () =>
            {
                var mSoundCueName = MasterDataManager.Instance.MemoryDatabase.SoundSheetNameMasterTable.First(x => x.Category == ESoundCategory.Se.GetLength() && x.Id == (int)type);
                var sheet = await GetOrAddCueSheetAsync(mSoundCueName.SheetName, "sounds/song/songse");

                StopSe();
                SeExPlayer.SetCue(sheet.acb, mSoundCueName.CueName);
                SeExPlayer.SetVolume(!LocalDataManager.Instance.Option.IsMuteSe ? SeExPlayer.GetParameterFloat32(CriAtomEx.Parameter.Volume) : 0f);
                SeExPlayer.Loop(false);
                SeExPlayer.Start();
            });
        }

        public void PlayBgm(EBgmType type, bool isLoop = true)
        {
            if (!IsInit) return;

            UniTask.Void(async () =>
            {
                var mSoundCueName = MasterDataManager.Instance.MemoryDatabase.SoundSheetNameMasterTable.First(x => x.Category == ESoundCategory.Bgm.GetLength() && x.Id == (int)type);
                var sheet = await GetOrAddCueSheetAsync(mSoundCueName.SheetName, "sounds/bgm/bgm");

                StopBgm();
                BgmExPlayer.SetCue(sheet.acb, mSoundCueName.CueName);
                BgmExPlayer.SetVolume(!LocalDataManager.Instance.Option.IsMuteBgm ? BgmExPlayer.GetParameterFloat32(CriAtomEx.Parameter.Volume) : 0f);
                BgmExPlayer.Loop(isLoop);
                BgmExPlayer.Start();
            });
        }

        public void PlaySong(int id)
        {
            if (!IsInit) return;

            UniTask.Void(async () =>
            {
                var mSoundCueName = MasterDataManager.Instance.MemoryDatabase.SoundSheetNameMasterTable.First(x => x.Category == ESoundCategory.Song.GetLength());
                var sheet = await GetOrAddCueSheetAsync(mSoundCueName.SheetName, ZString.Format("sounds/song/song_{0}", id));

                StopSong();
                SongExPlayer.SetCue(sheet.acb, id.ToString());
                SongExPlayer.SetVolume(!LocalDataManager.Instance.Option.IsMuteSong ? SongExPlayer.GetParameterFloat32(CriAtomEx.Parameter.Volume) : 0f);
                SongExPlayer.Loop(false);
                SongExPlayer.Start();
            });
        }

        public void PlaySongPreview(int id, CancellationToken token = default)
        {
            if (!IsInit) return;

            UniTask.Void(async () =>
            {
                var mSoundCueName = MasterDataManager.Instance.MemoryDatabase.SoundSheetNameMasterTable.First(x => x.Category == ESoundCategory.Song.GetLength());
                var sheet = await GetOrAddCueSheetAsync(mSoundCueName.SheetName, ZString.Format("sounds/song/song_{0}", id));

                SongPreviewExPlayer.AttachFader();
                SongPreviewExPlayer.SetFadeInTime(3000);
                SongPreviewExPlayer.SetFadeOutTime(3000);
                SongPreviewExPlayer.SetCue(sheet.acb, id.ToString());
                SongPreviewExPlayer.SetVolume(!LocalDataManager.Instance.Option.IsMuteSong ? SongExPlayer.GetParameterFloat32(CriAtomEx.Parameter.Volume) : 0f);
                SongPreviewExPlayer.SetStartTime(MasterDataManager.Instance.MemoryDatabase.SongSelectMasterTable.FindByGroup(id).StartSongTime);
                SongPreviewExPlayer.Start();
                var songSelectMaster = MasterDataManager.Instance.MemoryDatabase.SongSelectMasterTable.FindByGroup(id);
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
                    SongPreviewExPlayer.Stop(true);
                }
            });
        }

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
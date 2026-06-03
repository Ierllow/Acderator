using Cysharp.Threading.Tasks;
using Intense;
using Intense.Master;
using Song;
using System;
using System.Threading;
using Zenject;

namespace SongSelect
{
    public sealed class SongSelectSoundController
    {
        [Inject] private readonly MasterDataManager masterDataManager;
        [Inject] private readonly CriAtomCueSheetLoader cueSheetLoader;
        [Inject] private readonly SoundSheetNameResolver soundSheetNameResolver;
        [Inject] private readonly SongVolumeController songVolumeController;

        private readonly CriAtomSoundPlayer songPreviewExPlayer = new();
        private CancellationTokenSource songPreviewCancellationTokenSource;

        public void PlayPreview(int group, CancellationToken token) => UniTask.Void(async () =>
        {
            StopPreview();

            songPreviewCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            var previewToken = songPreviewCancellationTokenSource.Token;
            var mSoundCueName = soundSheetNameResolver.Song;
            var sheet = await cueSheetLoader.GetOrAddCueSheetAsync(mSoundCueName.SheetName, string.Format("sounds/song/song_{0}", group));
            var songSelectMaster = masterDataManager.MemoryDatabase.SongSelectMasterTable.FindByGroup(group);
            songVolumeController.Register(songPreviewExPlayer);
            songPreviewExPlayer.AttachFader();
            songPreviewExPlayer.SetFadeInTime(3000);
            songPreviewExPlayer.SetFadeOutTime(3000);
            songPreviewExPlayer.Play(sheet, group.ToString(), songSelectMaster.StartSongTime);
            try
            {
                while (!previewToken.IsCancellationRequested)
                {
                    await UniTask.WaitUntil(() => songPreviewExPlayer.Time > songSelectMaster.StartSongTime + songSelectMaster.SongTime, cancellationToken: previewToken);
                    await UniTask.WaitWhile(() => songPreviewExPlayer.IsFading, cancellationToken: previewToken);
                    songPreviewExPlayer.SetFadeInStartOffset(6000);
                    songPreviewExPlayer.Start();
                }
            }
            catch (OperationCanceledException)
            {
                StopPreview();
            }
        });

        public void StopPreview()
        {
            songPreviewCancellationTokenSource?.Cancel();
            songPreviewCancellationTokenSource?.Dispose();
            songPreviewCancellationTokenSource = default;
            songPreviewExPlayer.Stop();
            songVolumeController.Unregister(songPreviewExPlayer);
        }
    }
}
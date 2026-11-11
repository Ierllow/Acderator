using Cysharp.Threading.Tasks;
using Intense;
using Intense.Asset;
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
        [Inject] private readonly SongSoundSheetNameResolver soundSheetNameResolver;
        [Inject] private readonly SongVolumeController songVolumeController;

        // Lazy: CriAtomSoundPlayer's constructor creates a native CRI player and
        // throws where the CRI runtime is unavailable (e.g. Linux editor).
        private CriAtomSoundPlayer songPreviewExPlayer;
        private CancellationTokenSource songPreviewCancellationTokenSource;

        public void PlayPreview(int group, CancellationToken token) => UniTask.Void(async () =>
        {
#if UNITY_EDITOR
            // CRI Ware has no Linux editor plugin, so CriAtom.instance stays null
            // and any CriAtom call throws NullReferenceException. Skip the preview
            // in the editor only; device behavior is unchanged.
            await UniTask.CompletedTask;
            return;
#endif
            StopPreview();

            songPreviewCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            var previewToken = songPreviewCancellationTokenSource.Token;
            var soundCue = soundSheetNameResolver.Song;
            var sheet = await cueSheetLoader.GetOrAddCueSheetAsync(soundCue.SheetName, DynamicAssetAddresses.Song(group));
            var songSelectMaster = masterDataManager.MemoryDatabase.SongSelectMasterTable.FindByGroup(group);
            if (previewToken.IsCancellationRequested) return;

            songPreviewExPlayer ??= new CriAtomSoundPlayer();
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
                if (songPreviewCancellationTokenSource?.Token == previewToken) StopPreview();
            }
        });

        public void StopPreview()
        {
            songPreviewCancellationTokenSource?.Cancel();
            songPreviewCancellationTokenSource?.Dispose();
            songPreviewCancellationTokenSource = default;
            if (songPreviewExPlayer == null) return;
            songPreviewExPlayer.Stop();
            songVolumeController.Unregister(songPreviewExPlayer);
        }
    }
}
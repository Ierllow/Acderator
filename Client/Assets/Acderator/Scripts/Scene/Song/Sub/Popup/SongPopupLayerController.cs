#nullable enable

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Element.UI;
using Intense;
using Intense.UI;
using R3;
using UnityEngine;
using Zenject;

namespace Song
{
    public class SongPopupLayerController : MonoBehaviour
    {
        private enum PopupType { None, Pause, Error, ScoreError, ScoreData, TutorialSkip }

        [SerializeField] private PausePopup pausePopup = default!;
        [SerializeField] private ScoreErrorPopup errorPopup = default!;

        [Inject] private readonly PopupManager popupManager = default!;
        [Inject] private readonly SceneErrorPopupController sceneErrorPopupController = default!;

        public IUniTaskAsyncEnumerable<PopupTapKind> ClosedPausePopupAsAsyncEnumerable => UniTaskAsyncEnumerable.EveryValueChanged(pausePopup, x => x.TapKind);
        public Observable<SceneType> EverySceneTypeChanged => asyncSceneTypeSubject.Where(x => x != SceneType.None);

        private readonly Subject<SceneType> asyncSceneTypeSubject = new();

        private PopupType currentOpenPopupType = PopupType.None;

        public void OnOpenPausePopup(bool showRestartButton)
        {
            if (currentOpenPopupType == PopupType.Pause) return;

            currentOpenPopupType = PopupType.Pause;
            pausePopup.Open(showRestartButton, () => currentOpenPopupType = PopupType.None);
        }

        public async UniTask<bool> OpenTutorialSkipPopup()
        {
            if (currentOpenPopupType == PopupType.TutorialSkip) return false;

            currentOpenPopupType = PopupType.TutorialSkip;
            var completionSource = AutoResetUniTaskCompletionSource<CommonPopupTapKind>.Create();
            void Complete(CommonPopupTapKind tapKind)
            {
                currentOpenPopupType = PopupType.None;
                completionSource.TrySetResult(tapKind);
            }
            Open(new CommonPopupContext
            {
                Title = "確認",
                Text = "チュートリアルをスキップしますか？",
                PositiveText = "OK",
                NegativeText = "キャンセル",
                PositiveCallback = () => Complete(CommonPopupTapKind.Positive),
                NegativeCallback = () => Complete(CommonPopupTapKind.Negative),
                ButtonType = PopupButtonType.Both,
            });
            return await completionSource.Task == CommonPopupTapKind.Positive;
        }

        public void OnOpenScoreErrorPopup(bool isLoadFailure)
        {
            if (currentOpenPopupType == PopupType.ScoreError) return;

            currentOpenPopupType = PopupType.ScoreError;
            errorPopup.Open(isLoadFailure, () =>
            {
                currentOpenPopupType = PopupType.None;
                asyncSceneTypeSubject.OnNext(SceneType.SongSelect);
            });
        }

        public async UniTask OnOpenErrorPopup()
        {
            if (currentOpenPopupType == PopupType.Error) return;

            currentOpenPopupType = PopupType.Error;
            await sceneErrorPopupController.OpenAsync();
            currentOpenPopupType = PopupType.None;
        }

        public async UniTask<bool> OpenSaveScoreDataErrorPopup()
        {
            if (currentOpenPopupType == PopupType.ScoreData) return false;

            currentOpenPopupType = PopupType.ScoreData;
            var completionSource = AutoResetUniTaskCompletionSource<bool>.Create();
            void Complete(bool retry)
            {
                currentOpenPopupType = PopupType.None;
                completionSource.TrySetResult(retry);
            }
            void OpenErrorPopup() => Open(new CommonPopupContext
            {
                ButtonType = PopupButtonType.Both,
                Title = "エラー",
                Text = "通信エラーが発生しました。\n 再度実行しますか。",
                PositiveText = "リトライ",
                NegativeText = "キャンセル",
                PositiveCallback = () => Complete(true),
                NegativeCallback = OpenQuitConfirm,
            });
            void OpenQuitConfirm() => Open(new CommonPopupContext
            {
                ButtonType = PopupButtonType.Both,
                Title = "確認",
                Text = "選曲画面に戻ります。\n ただし、スコア等の記録は残りません。\n 本当によろしいですか。",
                PositiveText = "OK",
                NegativeText = "キャンセル",
                PositiveCallback = () => Complete(false),
                NegativeCallback = OpenErrorPopup,
            });
            OpenErrorPopup();
            return await completionSource.Task;
        }

        private void Open<TPopup>(PopupContext<TPopup> popupContext) where TPopup : PopupBase
        {
            var popup = popupManager.OpenPopup(popupContext);
            popup.transform.SetParent(transform);
        }
    }
}
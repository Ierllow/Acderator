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
        private enum EType { None, Pause, Error, ScoreError, ScoreData, TutorialSkip }

        [SerializeField] private PausePopup pausePopup = default!;
        [SerializeField] private ScoreErrorPopup errorPopup = default!;

        [Inject] private readonly PopupManager popupManager = default!;
        [Inject] private readonly SceneManager sceneManager = default!;

        public IUniTaskAsyncEnumerable<EPopupTapKind> ClosedPausePopupAsAsyncEnumerable => UniTaskAsyncEnumerable.EveryValueChanged(pausePopup, x => x.TapKind);
        public Observable<ESceneType> EverySceneTypeChanged => asyncSceneTypeSubject.Where(x => x != ESceneType.None);

        private readonly Subject<ESceneType> asyncSceneTypeSubject = new();

        private EType currentOpenPopupType = EType.None;

        public void OnOpenPausePopup(bool showRestartButton)
        {
            if (currentOpenPopupType == EType.Pause) return;

            currentOpenPopupType = EType.Pause;
            pausePopup.Open(showRestartButton, () => currentOpenPopupType = EType.None);
        }

        public async UniTask<bool> OpenTutorialSkipPopup()
        {
            if (currentOpenPopupType == EType.TutorialSkip) return false;

            currentOpenPopupType = EType.TutorialSkip;
            var completionSource = AutoResetUniTaskCompletionSource<ECommonPopupTapKind>.Create();
            void Complete(ECommonPopupTapKind tapKind)
            {
                currentOpenPopupType = EType.None;
                completionSource.TrySetResult(tapKind);
            }
            Open(new CommonPopupContext
            {
                Title = "確認",
                Text = "チュートリアルをスキップしますか？",
                PositiveText = "OK",
                NegativeText = "キャンセル",
                PositiveCallback = () => Complete(ECommonPopupTapKind.Positive),
                NegativeCallback = () => Complete(ECommonPopupTapKind.Negative),
                ButtonType = EButtonType.Both,
            });
            return await completionSource.Task == ECommonPopupTapKind.Positive;
        }

        public void OnOpenScoreErrorPopup(bool isLoadFailure)
        {
            if (currentOpenPopupType == EType.ScoreError) return;

            currentOpenPopupType = EType.ScoreError;
            errorPopup.Open(isLoadFailure, () =>
            {
                currentOpenPopupType = EType.None;
                asyncSceneTypeSubject.OnNext(ESceneType.SongSelect);
            });
        }

        public async UniTask OnOpenErrorPopup()
        {
            if (currentOpenPopupType == EType.Error) return;

            currentOpenPopupType = EType.Error;
            var completionSource = AutoResetUniTaskCompletionSource.Create();
            var context = PopupContextFactory.CreateErrorPopupContext(completionSource, () => sceneManager.ChangeSceneAsync(ESceneType.Title));
            popupManager.OpenPopup(context);
            await completionSource.Task;
        }

        public async UniTask<bool> OpenSaveScoreDataErrorPopup()
        {
            if (currentOpenPopupType == EType.ScoreData) return false;

            currentOpenPopupType = EType.ScoreData;
            var completionSource = AutoResetUniTaskCompletionSource<bool>.Create();
            void Complete(bool retry)
            {
                currentOpenPopupType = EType.None;
                completionSource.TrySetResult(retry);
            }
            void OpenErrorPopup() => Open(new CommonPopupContext
            {
                ButtonType = EButtonType.Both,
                Title = "エラー",
                Text = "通信エラーが発生しました。\n 再度実行しますか。",
                PositiveText = "リトライ",
                NegativeText = "キャンセル",
                PositiveCallback = () => Complete(true),
                NegativeCallback = OpenQuitConfirm,
            });
            void OpenQuitConfirm() => Open(new CommonPopupContext
            {
                ButtonType = EButtonType.Both,
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
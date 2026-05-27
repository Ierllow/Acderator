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

        [SerializeField] private PausePopup pausePopup;
        [SerializeField] private ScoreErrorPopup errorPopup;

        [Inject] private readonly PopupManager popupManager;
        [Inject] private readonly SceneManager sceneManager;

        public IUniTaskAsyncEnumerable<EPopupTapKind> ClosedPausePopupAsAsyncEnumerable => UniTaskAsyncEnumerable.EveryValueChanged(pausePopup, x => x.TapKind);
        public Observable<ESceneType> EverySceneTypeChanged => asyncSceneTypeSubject.Where(x => x != ESceneType.None);

        private readonly Subject<ESceneType> asyncSceneTypeSubject = new();

        private EType currentOpenPopupType = EType.None;

        public async UniTask DetectedError(ISongError iSongError)
        {
            switch (iSongError)
            {
                case ScoreSaveError:
                    OnOpenSaveScoreDataErrorPopup();
                    break;
                case ScoreLoadError scoreLoadError:
                    OnOpenScoreErrorPopup(scoreLoadError.Result);
                    break;
                case AlertError:
                    await OnOpenErrorPopup();
                    break;
                default:
                    break;
            }
        }

        public void OnOpenPausePopup(bool showRestartButton)
        {
            if (currentOpenPopupType == EType.Pause) return;

            currentOpenPopupType = EType.Pause;
            pausePopup.Open(showRestartButton, () => currentOpenPopupType = EType.None);
        }

        public async UniTask<bool> OpenTutorialSkipConfirm()
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

        private void OnOpenScoreErrorPopup(ELoadResult loadResult)
        {
            if (currentOpenPopupType == EType.ScoreError) return;

            currentOpenPopupType = EType.ScoreError;
            errorPopup.Open(loadResult, () =>
            {
                currentOpenPopupType = EType.None;
                asyncSceneTypeSubject.OnNext(ESceneType.SongSelect);
            });
        }

        private async UniTask OnOpenErrorPopup()
        {
            if (currentOpenPopupType == EType.Error) return;

            currentOpenPopupType = EType.Error;
            var completionSource = AutoResetUniTaskCompletionSource.Create();
            var context = PopupContextFactory.CreateErrorPopupContext(completionSource, () => sceneManager.ChangeSceneAsync(ESceneType.Title));
            popupManager.OpenPopup(context);
            await completionSource.Task;
        }

        private void OnOpenSaveScoreDataErrorPopup()
        {
            if (currentOpenPopupType == EType.ScoreData) return;

            currentOpenPopupType = EType.ScoreData;
            var popupContext = new CommonPopupContext
            {
                Title = "エラー",
                Text = "通信エラーが発生しました。\n 再度実行しますか。",
                PositiveText = "リトライ",
                NegativeText = "キャンセル",
                PositiveCallback = () => asyncSceneTypeSubject.OnNext(ESceneType.None),
                NegativeCallback = () => Open(new CommonPopupContext
                {
                    ButtonType = EButtonType.Close,
                    Title = "確認",
                    Text = "選曲画面に戻ります。\n ただし、スコア等の記録は残りません。\n 本当によろしいですか。",
                    PositiveText = "OK",
                    PositiveCallback = () =>
                    {
                        currentOpenPopupType = EType.None;
                        asyncSceneTypeSubject.OnNext(ESceneType.SongSelect);
                    },
                })
            };
            Open(popupContext);
        }

        private void Open(PopupContext popupContext)
        {
            popupManager.OpenPopup(popupContext);
            popupManager.CurrentOpenPopup.transform.SetParent(transform);
        }
    }
}
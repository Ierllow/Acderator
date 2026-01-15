using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Element.UI;
using Intense;
using Intense.Asset;
using Intense.UI;
using R3;
using System;
using UnityEngine;

namespace Song
{
    public class SongPopupLayerController : MonoBehaviour
    {
        private enum EType { None, Pause, Error, ScoreError, ScoreData }

        [SerializeField] private PausePopup pausePopup;
        [SerializeField] private ScoreErrorPopup errorPopup;

        public IUniTaskAsyncEnumerable<EPopupTapKind> ClosedPausePopupAsAsyncEnumerable => UniTaskAsyncEnumerable.EveryValueChanged(pausePopup, x => x.TapKind);
        public Observable<ESceneType> EverySceneTypeChanged => asyncSceneTypeSubject.Where(x => !x.EnumEquals(ESceneType.None));

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
            if (currentOpenPopupType.EnumEquals(EType.Pause)) return;

            currentOpenPopupType = EType.Pause;
            pausePopup.Open(showRestartButton, () => currentOpenPopupType = EType.None);
        }

        private void OnOpenScoreErrorPopup(ELoadResult loadResult)
        {
            if (currentOpenPopupType.EnumEquals(EType.ScoreError)) return;

            currentOpenPopupType = EType.ScoreError;
            errorPopup.Open(loadResult, () =>
            {
                currentOpenPopupType = EType.None;
                asyncSceneTypeSubject.OnNext(ESceneType.SongSelect);
            });
        }

        private async UniTask OnOpenErrorPopup()
        {
            if (currentOpenPopupType.EnumEquals(EType.Error)) return;

            currentOpenPopupType = EType.Error;
            if (!SceneManager.Instance.IsFadeIn) await SceneManager.Instance.FadeInAsync();
            await AutoResetUniTaskCompletionSource.Create().OpenErrorPopup();
        }

        private void OnOpenSaveScoreDataErrorPopup()
        {
            if (currentOpenPopupType.EnumEquals(EType.ScoreData)) return;

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
            PopupManager.Instance.OpenPopup(popupContext);
            PopupManager.Instance.CurrentOpenPopup.transform.SetParent(transform);
        }
    }
}
using Intense;
using Intense.Api;
using Intense.UI;
using R3;
using System;
using UnityEngine;

namespace Element.UI
{
    public sealed class MenuPopupContext : PopupContext
    {
        public new Action<ESceneType> NegativeCallback { get; init; }
    }

    public class MenuPopup : PopupBase
    {
        [SerializeField] private CommonButton configButton;
        [SerializeField] private CommonButton deleteAccountButton;
        [SerializeField] private CommonButton licenseButton;
        [SerializeField] private CommonButton titleSceneButton;

        private new Action<ESceneType> closeCallback;

        private readonly ReactiveProperty<bool> gate = new(true);

        private void Start()
        {
            configButton.OnTapButtonAsObservable.SubscribeLock(gate, _ => PopupManager.Instance.OpenPopup(new ConfigPopupContext())).RegisterTo(destroyCancellationToken);
            deleteAccountButton.OnTapButtonAsObservable.SubscribeLock(gate, _ => TapDeleteAccountButton()).RegisterTo(destroyCancellationToken);
            licenseButton.OnTapButtonAsObservable.SubscribeLock(gate, _ => TapLicenseButton()).RegisterTo(destroyCancellationToken);
            titleSceneButton.OnTapButtonAsObservable.SubscribeLock(gate, _ => TapTitleSceneButton()).RegisterTo(destroyCancellationToken);
        }

        public void Open(MenuPopupContext menuPopupContext)
        {
            closeCallback = menuPopupContext.NegativeCallback;
            base.Open();
        }

        private void TapDeleteAccountButton()
        {
            var context = new CommonPopupContext
            {
                Title = "データ削除",
                Text = "データを削除すると\n" +
                "プレイヤーデータや設定がすべて削除され\n" +
                "現在プレイー中のデータで遊ぶことが出来なくなります\n\n" +
                "現在のデータを削除しますか？\n\n" +
                "<color=\"red\">※削除後のデータ復旧・データの引継ぎは出来ません。</color>",
                PositiveText = "削除",
                NegativeText = "キャンセル",
                PositiveCallback = () =>
                {
                    PlayerPrefs.DeleteAll();
                    base.Close();
                    closeCallback.Invoke(ESceneType.Title);
                },
                ButtonType = EButtonType.Both,
            };
            PopupManager.Instance.OpenPopup(context);
        }

        private async void TapLicenseButton()
        {
            var title = "権利表記";
            var url = await NetworkManager.Instance.GetUrl("lisence.html");
            PopupManager.Instance.OpenPopup(new WebViewPopupContext { TitleText = title, Url = url });
        }

        private void TapTitleSceneButton()
        {
            closeCallback.Invoke(ESceneType.Title);
            base.Close();
        }

        protected override void FinishClosePopupScale()
        {
            base.FinishClosePopupScale();
            Destroy(gameObject);
        }
    }
}
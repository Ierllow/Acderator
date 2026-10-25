using Intense;
using Intense.Api;
using Intense.UI;
using R3;
using System;
using UnityEngine;
using Zenject;

namespace Element.UI
{
    public sealed class MenuPopupContext : PopupContext<MenuPopup>
    {
        public new Action<SceneType> NegativeCallback { get; init; }
    }

    public class MenuPopup : PopupBase<MenuPopupContext>
    {
        [SerializeField] private CommonButton configButton;
        [SerializeField] private CommonButton deleteAccountButton;
        [SerializeField] private CommonButton licenseButton;
        [SerializeField] private CommonButton titleSceneButton;
        [SerializeField] private NetworkConfig networkConfigObject;

        [Inject] private readonly PopupManager popupManager;

        private new Action<SceneType> closeCallback;

        private void Start()
        {
            configButton.OnTapButtonAsObservable.SubscribeLock(_ => popupManager.OpenPopup(new ConfigPopupContext())).RegisterTo(destroyCancellationToken);
            deleteAccountButton.OnTapButtonAsObservable.SubscribeLock(_ => TapDeleteAccountButton()).RegisterTo(destroyCancellationToken);
            licenseButton.OnTapButtonAsObservable.SubscribeLock(_ => TapLicenseButton()).RegisterTo(destroyCancellationToken);
            titleSceneButton.OnTapButtonAsObservable.SubscribeLock(_ => TapTitleSceneButton()).RegisterTo(destroyCancellationToken);
        }

        public override void Open(MenuPopupContext menuPopupContext)
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
                    closeCallback.Invoke(SceneType.Title);
                },
                ButtonType = PopupButtonType.Both,
            };
            popupManager.OpenPopup(context);
        }

        private async void TapLicenseButton()
        {
            var title = "権利表記";
            var url = networkConfigObject.WebViewServerUrl + "/lisence.html";
            popupManager.OpenPopup(new WebViewPopupContext { TitleText = title, Url = url });
        }

        private void TapTitleSceneButton()
        {
            closeCallback.Invoke(SceneType.Title);
            base.Close();
        }
    }
}
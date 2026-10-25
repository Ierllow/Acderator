#nullable enable

using Intense.UI;
using TMPro;
using UnityEngine;

namespace Song
{
    public class ScoreErrorPopup : PopupBase
    {
        [SerializeField] private TextMeshProUGUI errorText = default!;

        public void Open(bool isLoadFailure, System.Action closeCallback)
        {
            errorText.SetText(isLoadFailure ? "譜面情報の読み込みに失敗しました。" : "譜面情報を取得出来ませんでした。");
            base.Open(closeCallback);
        }
    }
}
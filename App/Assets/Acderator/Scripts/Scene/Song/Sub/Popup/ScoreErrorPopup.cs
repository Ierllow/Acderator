#nullable enable

using Intense.UI;
using TMPro;
using UnityEngine;

namespace Song
{
    public class ScoreErrorPopup : PopupBase
    {
        [SerializeField] private TextMeshProUGUI errorText = default!;

        public void Open(ELoadResult type, System.Action closeCallback)
        {
            errorText.SetText(type switch
            {
                ELoadResult.Unknown => "譜面情報を取得出来ませんでした。",
                ELoadResult.Unsupported or ELoadResult.Exception or ELoadResult.InvalidAsset => "譜面情報の読み込みに失敗しました。",
                _ => "",
            });
            base.Open(closeCallback);
        }
    }
}

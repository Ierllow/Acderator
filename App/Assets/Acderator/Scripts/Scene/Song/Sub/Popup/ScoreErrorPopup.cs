using Cysharp.Text;
using Intense.UI;
using TMPro;
using UnityEngine;

namespace Song
{
    public class ScoreErrorPopup : PopupBase
    {
        [SerializeField] private TextMeshProUGUI errorText;

        public void Open(ELoadResult type, System.Action closeCallback)
        {
            errorText.SetTextFormat("{0}", type switch
            {
                ELoadResult.Unknown => "譜面情報を取得出来ませんでした。",
                ELoadResult.Unsupported or ELoadResult.Exception => "譜面情報の読み込みに失敗しました。",
                _ => "",
            });
            base.Open(closeCallback);
        }
    }
}
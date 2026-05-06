#if UNITY_EDITOR
using Intense;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Song
{
    public class DebugInfoView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI debugText;
        [SerializeField] private DebugConfig debugConfig;

        private void Awake() => panel.SetActive(debugConfig.isDebug);

        public void UpdateDebugInfo(ENoteType type, float diff, float currentBeat, Dictionary<EJudgementType, int> judgeCountDict)
        {
            if (!debugConfig.isDebug) return;

            var sb = new StringBuilder();

            sb.Append("Perfect:");
            sb.AppendLine(judgeCountDict.TryGetValue(EJudgementType.Perfect, out var perfect) ? perfect.ToString() : "0");
            sb.Append("Great:");
            sb.AppendLine(judgeCountDict.TryGetValue(EJudgementType.Great, out var great) ? great.ToString() : "0");
            sb.Append("Good:");
            sb.AppendLine(judgeCountDict.TryGetValue(EJudgementType.Good, out var good) ? good.ToString() : "0");
            sb.Append("Bad:");
            sb.AppendLine(judgeCountDict.TryGetValue(EJudgementType.Bad, out var bad) ? bad.ToString() : "0");
            sb.Append("Miss:");
            sb.AppendLine(judgeCountDict.TryGetValue(EJudgementType.Miss, out var miss) ? miss.ToString() : "0");
            sb.Append("Tap Timing:");
            sb.AppendLine(diff.ToString());
            sb.Append("Note Type:");
            sb.AppendLine(type.ToString());
            sb.Append("CurrentBeat:");
            sb.Append(currentBeat);
            debugText.SetText(sb);
        }
    }
}
#endif
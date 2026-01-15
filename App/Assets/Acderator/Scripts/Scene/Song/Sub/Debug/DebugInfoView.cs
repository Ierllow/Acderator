#if UNITY_EDITOR
using Cysharp.Text;
using Intense;
using System.Collections.Generic;
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

            using var sb = new Utf16ValueStringBuilder(true);

            sb.Append("Perfect:");
            sb.AppendLine(judgeCountDict.TryGetValue(EJudgementType.Perfect, out var perfect) ? perfect : 0);
            sb.Append("Great:");
            sb.AppendLine(judgeCountDict.TryGetValue(EJudgementType.Great, out var great) ? great : 0);
            sb.Append("Good:");
            sb.AppendLine(judgeCountDict.TryGetValue(EJudgementType.Good, out var good) ? good : 0);
            sb.Append("Bad:");
            sb.AppendLine(judgeCountDict.TryGetValue(EJudgementType.Bad, out var bad) ? bad : 0);
            sb.Append("Miss:");
            sb.AppendLine(judgeCountDict.TryGetValue(EJudgementType.Miss, out var miss) ? miss : 0);
            sb.Append("Tap Timing:");
            sb.AppendLine(diff);
            sb.Append("Note Type:");
            sb.AppendLine(type.ToString());
            sb.Append("CurrentBeat:");
            sb.Append(currentBeat);
            debugText.SetText(sb);
        }
    }
}
#endif
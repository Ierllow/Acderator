using Intense.Master;
using System;
using System.Linq;

namespace Song
{
    internal enum TutorialNoteKind { Unknown, Tap, Flick, Long, Curve }

    internal class TutorialMessageResolver
    {
        private readonly string[] curveKeywords = { "曲線", "Curve" };
        private readonly string[] longKeywords = { "ロング", "Long" };
        private readonly string[] flickKeywords = { "フリック", "Flick" };
        private readonly string[] tapKeywords = { "タップ", "Tap" };

        public string GetStepHint(TutorialStepMaster step) => GetTutorialNoteKind(step) switch
        {
            TutorialNoteKind.Tap => "判定ラインでタップ",
            TutorialNoteKind.Flick => "方向へフリック",
            TutorialNoteKind.Long => "終点まで押し続ける",
            TutorialNoteKind.Curve => "曲線に沿ってなぞる",
            _ => "ノーツを見分ける",
        };

        private TutorialNoteKind GetTutorialNoteKind(TutorialStepMaster step) => true switch
        {
            _ when ContainsAny(step.Description, curveKeywords) => TutorialNoteKind.Curve,
            _ when ContainsAny(step.Description, longKeywords) => TutorialNoteKind.Long,
            _ when ContainsAny(step.Description, flickKeywords) => TutorialNoteKind.Flick,
            _ when ContainsAny(step.Description, tapKeywords) => TutorialNoteKind.Tap,
            _ => GetTutorialNoteKindByOrder(step.StepOrder),
        };

        private bool ContainsAny(string text, string[] keywords) => keywords.Any(x => text.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0);

        private TutorialNoteKind GetTutorialNoteKindByOrder(int stepOrder) => stepOrder switch
        {
            1 => TutorialNoteKind.Tap,
            2 => TutorialNoteKind.Flick,
            3 => TutorialNoteKind.Long,
            4 => TutorialNoteKind.Curve,
            _ => TutorialNoteKind.Unknown,
        };
    }
}
using Intense;
using System;

namespace Song
{
    public class SingleNote : NoteBase
    {
        public override void OnJudgedNote(EFingerType fingerType, EJudgementType judgmentType)
        {
            if (fingerType == EFingerType.Down && judgmentType != EJudgementType.None) Final();
        }
    }
}
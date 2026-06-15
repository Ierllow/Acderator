#nullable enable

using Intense;
using R3;

namespace Song
{
    public class ComboController
    {
        private readonly ReactiveProperty<int> currentCombo = new(0);
        public ReadOnlyReactiveProperty<int> CurrentCombo => currentCombo;

        public void UpdateCombo(EJudgementType judgementType)
        {
            switch (judgementType)
            {
                case EJudgementType.None: break;
                case EJudgementType.Perfect or EJudgementType.Great: currentCombo.Value++; break;
                default: currentCombo.Value = 0; break;
            }
        }
    }
}
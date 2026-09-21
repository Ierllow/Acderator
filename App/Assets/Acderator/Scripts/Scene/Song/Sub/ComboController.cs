#nullable enable

using Intense;
using R3;

namespace Song
{
    public class ComboController
    {
        private readonly ReactiveProperty<int> currentCombo = new(0);
        public ReadOnlyReactiveProperty<int> CurrentCombo => currentCombo;

        public void UpdateCombo(JudgementType judgementType)
        {
            switch (judgementType)
            {
                case JudgementType.None: break;
                case JudgementType.Perfect or JudgementType.Great: currentCombo.Value++; break;
                default: currentCombo.Value = 0; break;
            }
        }
    }
}
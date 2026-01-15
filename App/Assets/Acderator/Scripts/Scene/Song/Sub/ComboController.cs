using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Intense;

namespace Song
{
    public class ComboController
    {
        public int CurrentCombo { get; private set; } = 0;

        public IUniTaskAsyncEnumerable<int> EveryUpdateComboAsAsyncEnumerable => UniTaskAsyncEnumerable.EveryValueChanged(this, x => x.CurrentCombo);

        public void UpdateCombo(EJudgementType judgementType)
        {
            switch (judgementType)
            {
                case EJudgementType.None: break;
                case EJudgementType.Perfect or EJudgementType.Great: CurrentCombo++; break;
                default: CurrentCombo = 0; break;
            }
        }
    }
}
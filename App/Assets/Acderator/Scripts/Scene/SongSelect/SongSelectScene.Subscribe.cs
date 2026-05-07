using Cysharp.Threading.Tasks.Linq;
using Intense.Master;
using R3;

namespace SongSelect
{
    public partial class SongSelectScene
    {
        private void StartSubscribes()
        {
            StartButtonSubscribes();
            StartUISubscribes();
        }

        private void StartButtonSubscribes()
        {
            autoButton.SetToggleTextAsAsyncEnumerableForEachAsync(isOn => isOn ? "オートON" : "オートOFF");
            decideButton.OnTapButtonAsObservable.SubscribeLockAwait(async (_, __) => await TapDecideButton()).RegisterTo(destroyCancellationToken);
            orderButton.OnTapButtonAsObservable.SubscribeLock(_ => TapOrderButton()).RegisterTo(destroyCancellationToken);
        }

        private void StartUISubscribes()
        {
            songListView.EverySelectedCellChanged.Skip(1).Subscribe(SelectedCellChanged).RegisterTo(destroyCancellationToken);
            songSelectDetail.EveryToggleChanged.Skip(1).Where(x => x != null).Subscribe(SelectedDifficultChanged).RegisterTo(destroyCancellationToken);
        }
    }
}
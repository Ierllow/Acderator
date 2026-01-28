using Cysharp.Threading.Tasks.Linq;
using Intense.Master;
using R3;
using R3.Triggers;
using ZLinq;

namespace SongSelect
{
    public partial class SongSelectScene
    {
        private void StartSubscribes()
        {
            StartButtonSubscribes();
            StartUISubscribes();
            StartPauseSubscribes();
        }

        private void StartButtonSubscribes()
        {
            autoButton.SetToggleTextAsAsyncEnumerableForEachAsync(isOn => isOn ? "オートON" : "オートOFF");
            decideButton.OnTapButtonAsObservable.SubscribeLockAwait(new(true), async (_, __) => await TapDecideButton()).RegisterTo(destroyCancellationToken);
            orderButton.OnTapButtonAsObservable.SubscribeLock(new(true), _ => TapOrderButton()).RegisterTo(destroyCancellationToken);
        }

        private void StartUISubscribes()
        {
            songListView.EverySelectedCellChanged.Skip(1).Subscribe(SelectedCellChanged).RegisterTo(destroyCancellationToken);
            songSelectDetail.EveryToggleChanged.Skip(1).Where(x => x != null).Subscribe(SelectedDifficultChanged).RegisterTo(destroyCancellationToken);
        }

        private void StartPauseSubscribes()
        {
            this.OnApplicationPauseAsObservable().Where(x => x).Subscribe(_ => songListView.Save()).RegisterTo(destroyCancellationToken);
        }
    }
}

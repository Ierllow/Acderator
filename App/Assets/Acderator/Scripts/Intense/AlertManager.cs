using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using System.Linq;

namespace Intense
{
    public interface IAlertHandler
    {
        UniTask OnErrorScene();
    }

    public class AlertManager : SingletonMonoBehaviour<AlertManager>
    {
        public bool IsAlert { get; private set; } = false;

        private void Start() => this.OnAlertAsObservable().Where(x => x).SubscribeAwait(async (isAlert, _) =>
        {
            var sceneBaseDict = SceneManager.Instance.SceneBaseDict;
            var currentSceneType = SceneManager.Instance.CurrentSceneType;
            IsAlert = true;
            await UniTask.WhenAll(sceneBaseDict.Select(async x => await x.Value.OnErrorScene()));
            IsAlert = false;
        }).RegisterTo(destroyCancellationToken);
    }
}
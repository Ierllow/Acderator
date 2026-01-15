using Cysharp.Threading.Tasks;
using Intense;
using UnityEngine.Rendering;

namespace Boot
{
    public class BootScene : SceneBase
    {
        private async UniTask Start()
        {
            await UniTask.WaitUntil(() => SplashScreen.isFinished, cancellationToken: destroyCancellationToken);
            await SceneManager.Instance.ChangeSceneAsync(ESceneType.Title);
        }

        public override void OnCreateScene() { }
    }
}
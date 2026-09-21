using Cysharp.Threading.Tasks;
using Intense;
using Intense.Attribute;
using UnityEngine.Rendering;

namespace Boot
{
    [SceneType(SceneType.Boot)]
    public class BootScene : SceneBase
    {
        private void Start() => StartAsync().Forget();

        private async UniTask StartAsync()
        {
            await UniTask.WaitUntil(() => SplashScreen.isFinished, cancellationToken: destroyCancellationToken);
            await sceneManager.ChangeSceneAsync(SceneType.Title);
        }

        public override void OnCreateScene() { }
    }
}
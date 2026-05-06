using Cysharp.Threading.Tasks;
using Intense;
using Intense.Attribute;
using UnityEngine.Rendering;

namespace Boot
{
    [SceneType(ESceneType.Boot)]
    public class BootScene : SceneBase
    {
        private async UniTask Start()
        {
            await UniTask.WaitUntil(() => SplashScreen.isFinished, cancellationToken: destroyCancellationToken);
            await sceneManager.ChangeSceneAsync(ESceneType.Title);
        }

        public override void OnCreateScene() { }
    }
}
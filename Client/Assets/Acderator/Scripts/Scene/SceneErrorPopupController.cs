using Cysharp.Threading.Tasks;
using Intense;
using Intense.UI;
using Zenject;

public sealed class SceneErrorPopupController
{
    [Inject] private readonly SceneManager sceneManager;
    [Inject] private readonly PopupManager popupManager;

    public async UniTask OpenAsync()
    {
        var completionSource = AutoResetUniTaskCompletionSource.Create();
        var context = PopupContextFactory.CreateErrorPopupContext(completionSource, () => sceneManager.ChangeSceneAsync(SceneType.Title));
        popupManager.OpenPopup(context);
        await completionSource.Task;
    }
}
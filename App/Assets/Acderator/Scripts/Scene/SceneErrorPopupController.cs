using Cysharp.Threading.Tasks;
using Intense;
using Intense.UI;

public sealed class SceneErrorPopupController
{
    private readonly SceneManager sceneManager;
    private readonly PopupManager popupManager;

    public SceneErrorPopupController(SceneManager sceneManager, PopupManager popupManager)
    {
        this.sceneManager = sceneManager;
        this.popupManager = popupManager;
    }

    public async UniTask Open()
    {
        var completionSource = AutoResetUniTaskCompletionSource.Create();
        var context = PopupContextFactory.CreateErrorPopupContext(completionSource, () => sceneManager.ChangeSceneAsync(ESceneType.Title));
        popupManager.OpenPopup(context);
        await completionSource.Task;
    }
}
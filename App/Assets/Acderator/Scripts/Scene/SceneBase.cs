using Cysharp.Threading.Tasks;
using Intense;
using Intense.Attribute;
using UnityEngine;
using Zenject;

#region SceneContext
public abstract class SceneContext
{
    public virtual EBgmType BgmType { get; }
    public virtual int FrameRate { get; } = 30;

    protected SceneContext() { }
}
#endregion

[SceneType(ESceneType.None)]
public abstract class SceneBase : MonoBehaviour
{
    [Inject] protected readonly SceneManager sceneManager;
    [Inject] private readonly SceneErrorHandler sceneErrorHandler;
    [Inject] private readonly SceneErrorPopupController sceneErrorPopupController;

    #region MonoBehaviour Handlers
    protected virtual void Awake()
    {
        sceneManager.SetSceneBase(this);
        sceneErrorHandler.Register(OnErrorScene);
    }
    protected virtual void OnDestroy() => sceneErrorHandler.Dispose();
    #endregion

    #region SceneBase Handlers
    public abstract void OnCreateScene();
    public virtual void OnDeleteScene() => Destroy(gameObject);
    protected virtual UniTask OnErrorScene() => sceneErrorPopupController.Open();
    #endregion
}
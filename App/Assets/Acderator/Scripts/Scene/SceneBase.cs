using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Intense;
using Intense.Attribute;
using Intense.UI;
using R3;
using R3.Triggers;
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
    [Inject] protected SceneManager sceneManager;

    #region MonoBehaviour Handlers
    protected virtual void Awake() => sceneManager.SetSceneBase(this);
    protected virtual void Start()
    {
        this.OnAtlasRequestedAsObservable().Subscribe().RegisterTo(destroyCancellationToken);
        this.OnAlertAsObservable().Where(x => x).SubscribeAwait(async (isAlert, _) => await OnErrorScene()).RegisterTo(destroyCancellationToken);
    }
    #endregion

    #region SceneBase Handlers
    public abstract void OnCreateScene();
    public virtual void OnDeleteScene() => Destroy(gameObject);
    protected virtual async UniTask OnErrorScene() => await PopupUtils.OpenErrorPopup();
    #endregion
}

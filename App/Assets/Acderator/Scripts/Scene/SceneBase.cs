using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Intense;
using Intense.UI;
using R3;
using R3.Triggers;
using UnityEngine;

#region SceneContext
public abstract class SceneContext
{
    protected SceneContext() { }
}
#endregion

public abstract class SceneBase : MonoBehaviour
{
    #region MonoBehaviour Handlers
    protected virtual void Awake() => SceneManager.Instance.SetSceneBase(this);
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
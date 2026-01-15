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

public abstract class SceneBase : MonoBehaviour, IAlertHandler
{
    #region MonoBehaviour Handlers
    protected virtual void Awake() => SceneManager.Instance.SetSceneBase(this);
    protected virtual void Start() => this.OnAtlasRequestedAsObservable().Subscribe().RegisterTo(destroyCancellationToken);
    #endregion

    #region SceneBase Handlers
    public abstract void OnCreateScene();
    public virtual void OnDeleteScene() => Destroy(gameObject);
    public async UniTask OnErrorScene() => await AutoResetUniTaskCompletionSource.Create().OpenErrorPopup();
    #endregion
}
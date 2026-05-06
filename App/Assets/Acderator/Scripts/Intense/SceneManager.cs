using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Element;
using Intense.Asset;
using Intense.Attribute;
using Intense.Master;
using Intense.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace Intense
{
    public enum ESceneType { None, Boot, Title, SongSelect, Song, Result }

    public class SceneManager : MonoBehaviour
    {
        private sealed class DefaultSceneContext : SceneContext { }

        [SerializeField] private Image fadeMask;
        [SerializeField] private Header header;

        [Inject] private ZenjectSceneLoader zenjectSceneLoader;
        [Inject] private AssetBundleManager assetBundleManager;
        [Inject] private SoundManager soundManager;
        [Inject] private Loading loading;

        public ESceneType CurrentSceneType => sceneBaseDict.Count > 0 ? sceneBaseDict.LastOrDefault().Key : default;
        public bool IsFadeIn { get; private set; } = false;

        private readonly Dictionary<ESceneType, SceneBase> sceneBaseDict = new();

        private void Start() => UniTaskAsyncEnumerable.EveryValueChanged(this, x => x.fadeMask.color.a).Queue().ForEachAsync(a =>
        {
            if (a == 0.0f)
            {
                IsFadeIn = true;
                header.SetHeaderActive(CurrentSceneType == ESceneType.SongSelect);
                loading.HideLoading();
            }
            else if (a == 1.0f)
            {
                IsFadeIn = false;
                header.SetHeaderActive(false);
                loading.ShowLoading();
            }
        });

        public async UniTask FadeInAsync() => await fadeMask.DOFade(0.0f, 0.2f);

        public async UniTask FadeOutAsync() => await fadeMask.DOFade(1.0f, 0.2f);

        public void SetSceneBase(SceneBase scene)
        {
            var type = scene.GetType();
            var sceneType = type.GetCustomAttribute<SceneTypeAttribute>().Type;
            if (sceneType == ESceneType.None) throw new InvalidSceneTypeException(sceneType);
            if (!sceneBaseDict.TryAdd(sceneType, scene)) throw new DuplicateSceneTypeException(sceneType);
        }

        public async UniTask ChangeSceneAsync(ESceneType sceneType, SceneContext context = default, bool sameScene = false)
        {
            context ??= new DefaultSceneContext();
            if (CurrentSceneType != sceneType || sameScene)
            {
                await FadeOutAsync();

                foreach (var kvp in sceneBaseDict) kvp.Value.OnDeleteScene();

                if (!sameScene)
                {
                    await assetBundleManager.UnloadAssetsAsync(sceneBaseDict.Select(x => x.Key).ToList());
                    await Resources.UnloadUnusedAssets();
                }

                sceneBaseDict.Clear();

                await zenjectSceneLoader.LoadSceneAsync(sceneType.ToString(), extraBindings: container => container.Bind<SceneContext>().FromInstance(context).AsSingle()).ToUniTask();
                sceneBaseDict.GetValueOrDefault(sceneType)?.OnCreateScene();
                Application.targetFrameRate = context.FrameRate;
                soundManager.UpdateSounds(context.BgmType);
                await UniTask.Yield();
                return;
            }
            Debug.LogWarning(string.Format("{0} is the same as before.", sceneType));
        }

        public async UniTask ChangeSceneAdditiveAsync(ESceneType sceneType, SceneContext context = default)
        {
            loading.ShowLoading();
            if (sceneBaseDict.ContainsKey(sceneType))
            {
                loading.HideLoading();
                return;
            }
            await zenjectSceneLoader.LoadSceneAsync(sceneType.ToString(), LoadSceneMode.Additive, container => container.Bind<SceneContext>().FromInstance(context).AsSingle()).ToUniTask();
            sceneBaseDict.GetValueOrDefault(sceneType)?.OnCreateScene();
            await UniTask.Yield();
        }
    }
}
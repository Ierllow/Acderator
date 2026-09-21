using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Element;
using Intense.UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Intense
{
    public enum SceneType { None, Boot, Title, SongSelect, Song, Result }

    public class SceneManager : MonoBehaviour
    {
        private sealed class DefaultSceneContext : SceneContext { }

        [SerializeField] private Image fadeMask;
        [SerializeField] private Header header;

        [Inject] private readonly SceneLoader sceneLoader;
        [Inject] private readonly SceneRegistry sceneRegistry;
        [Inject] private readonly SceneLifecycleDispatcher sceneLifecycleDispatcher;
        [Inject] private readonly Loading loading;

        public SceneType CurrentSceneType => sceneRegistry.CurrentSceneType;
        public bool IsFadeIn { get; private set; } = false;

        private void Start() => UniTaskAsyncEnumerable.EveryValueChanged(this, x => x.fadeMask.color.a).Queue().ForEachAsync(a =>
        {
            if (a == 0.0f)
            {
                IsFadeIn = true;
                header.SetHeaderActive(CurrentSceneType == SceneType.SongSelect);
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

        public async UniTask ChangeSceneAsync(SceneType sceneType, SceneContext context = default, bool sameScene = false)
        {
            context ??= new DefaultSceneContext();
            if (CurrentSceneType != sceneType || sameScene)
            {
                await FadeOutAsync();

                sceneRegistry.DeleteAll();

                if (!sameScene)
                {
                    await sceneLifecycleDispatcher.OnSceneUnloadingAsync();
                }

                sceneRegistry.Clear();

                await sceneLoader.LoadAsync(sceneType, context);
                sceneRegistry.AddLoaded(sceneType);
                sceneRegistry.Create(sceneType);
                sceneLifecycleDispatcher.OnSceneLoaded(context);
                await UniTask.Yield();
                return;
            }
            Debug.LogWarning(string.Format("{0} is the same as before.", sceneType));
        }

        public async UniTask ChangeSceneAdditiveAsync(SceneType sceneType, SceneContext context = default)
        {
            loading.ShowLoading();
            if (sceneRegistry.Contains(sceneType))
            {
                loading.HideLoading();
                return;
            }
            await sceneLoader.LoadAdditiveAsync(sceneType, context);
            sceneRegistry.AddLoaded(sceneType);
            sceneRegistry.Create(sceneType);
            await UniTask.Yield();
        }
    }
}
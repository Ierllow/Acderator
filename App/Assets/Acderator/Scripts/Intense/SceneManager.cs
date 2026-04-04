using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Element;
using Intense.Asset;
using Intense.Attribute;
using Intense.Master;
using Intense.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;
using ZLinq;

namespace Intense
{
    public enum ESceneType { None, Boot, Title, SongSelect, Song, Result }

    public class SceneManager : SingletonMonoBehaviour<SceneManager>
    {
        [SerializeField] private Image fadeMask;
        [SerializeField] private Header header;

        [Inject] private ZenjectSceneLoader zenjectSceneLoader;

        public ESceneType CurrentSceneType => sceneBaseDict.Count > 0 ? sceneBaseDict.AsValueEnumerable().LastOrDefault().Key : default;
        public bool IsFadeIn { get; private set; } = false;

        private readonly Dictionary<ESceneType, SceneBase> sceneBaseDict = new();

        private void Start() => UniTaskAsyncEnumerable.EveryValueChanged(this, x => x.fadeMask.color.a).Queue().ForEachAsync(a =>
        {
            if (a == 0.0f)
            {
                IsFadeIn = true;
                header.SetHeaderActive(CurrentSceneType.EnumEquals(ESceneType.SongSelect));
                Loading.Instance.HideLoading();
            }
            else if (a == 1.0f)
            {
                IsFadeIn = false;
                header.SetHeaderActive(false);
                Loading.Instance.ShowLoading();
            }
        });

        public async UniTask FadeInAsync() => await fadeMask.DOFade(0.0f, 0.2f);

        public async UniTask FadeOutAsync() => await fadeMask.DOFade(1.0f, 0.2f);

        public void SetSceneBase(SceneBase scene)
        {
            var type = scene.GetType();
            var sceneType = type.GetCustomAttribute<SceneTypeAttribute>().Type;
            if (sceneType.EnumEquals(ESceneType.None)) throw new InvalidSceneTypeException(sceneType);
            if (!sceneBaseDict.TryAdd(sceneType, scene)) throw new DuplicateSceneTypeException(sceneType);
        }

        public async UniTask ChangeSceneAsync(ESceneType sceneType, SceneContext context = default, bool sameScene = false)
        {
            if (!CurrentSceneType.EnumEquals(sceneType) || sameScene)
            {
                await FadeOutAsync();

                foreach (var kvp in sceneBaseDict) kvp.Value.OnDeleteScene();

                if (!sameScene)
                {
                    await AssetBundleManager.Instance.UnloadAssetsAsync(sceneBaseDict.AsValueEnumerable().Select(x => x.Key).ToList());
                    await Resources.UnloadUnusedAssets();
                }

                sceneBaseDict.Clear();

                await zenjectSceneLoader.LoadSceneAsync(sceneType.ToString(), extraBindings: container => container.Bind<SceneContext>().FromInstance(context).AsSingle()).ToUniTask();
                sceneBaseDict.GetValueOrDefault(sceneType)?.OnCreateScene();
                Application.targetFrameRate = context.FrameRate;
                SoundManager.Instance.UpdateSounds(context.BgmType);
                await UniTask.Yield();
                return;
            }
            Debug.LogWarning(ZString.Format("{0} is the same as before.", sceneType));
        }

        public async UniTask ChangeSceneAdditiveAsync(ESceneType sceneType, SceneContext context = default)
        {
            Loading.Instance.ShowLoading();
            if (sceneBaseDict.ContainsKey(sceneType))
            {
                Loading.Instance.HideLoading();
                return;
            }
            await zenjectSceneLoader.LoadSceneAsync(sceneType.ToString(), LoadSceneMode.Additive, container => container.Bind<SceneContext>().FromInstance(context).AsSingle()).ToUniTask();
            sceneBaseDict.GetValueOrDefault(sceneType)?.OnCreateScene();
            await UniTask.Yield();
        }
    }
}
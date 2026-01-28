using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Element;
using Intense.Asset;
using Intense.Master;
using Intense.UI;
using System;
using System.Collections.Generic;
using Unity.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;
using ZLinq;
using static UnityEngine.SceneManagement.SceneManager;

namespace Intense
{
    public enum ESceneType { None, Boot, Title, SongSelect, Song, Result }

    public class SceneManager : SingletonMonoBehaviour<SceneManager>
    {
        [SerializeField] private Image fadeMask;
        [SerializeField] private Header header;

        [Inject] private ZenjectSceneLoader zenjectSceneLoader;

        public ESceneType CurrentSceneType { get; private set; } = ESceneType.None;
        public Dictionary<ESceneType, SceneBase> SceneBaseDict { get; private set; } = new();
        public bool IsFadeIn { get; private set; } = false;

        private readonly IUniTaskAsyncEnumerable<ESceneType> everySceneTypeChanged = default;
        public IUniTaskAsyncEnumerable<ESceneType> EverySceneTypeChanged => everySceneTypeChanged ?? UniTaskAsyncEnumerable.EveryValueChanged(this, x => x.CurrentSceneType).Queue();

        private void Start()
        {
            EverySceneTypeChanged.ForEachAsync(sceneType =>
            {
                Application.targetFrameRate = sceneType.EnumEquals(ESceneType.Song) ? 60 : 30;
                SoundManager.Instance.UpdateSounds(sceneType switch
                {
                    ESceneType.Boot => EBgmType.None,
                    ESceneType.Title or ESceneType.SongSelect or ESceneType.Song or ESceneType.Result => EBgmType.Stop,
                    _ => throw new NotImplementedException("Missing ESceneType case in switch")
                });
            });
            UniTaskAsyncEnumerable.EveryValueChanged(this, x => x.fadeMask.color.a).Queue().ForEachAsync(a =>
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
        }

        public async UniTask FadeInAsync() => await fadeMask.DOFade(0.0f, 0.2f);

        public async UniTask FadeOutAsync() => await fadeMask.DOFade(1.0f, 0.2f);

        public void SetSceneBase(SceneBase scene)
        {
            var sceneName = scene.GetType().Name;
            var sceneType = default(ESceneType);
            var isAdded = sceneName.Contains("Scene") && Enum.TryParse(sceneName.Replace("Scene", ""), out sceneType) && SceneBaseDict.TryAdd(sceneType, scene);
            CurrentSceneType = isAdded ? sceneType : throw new ParseErrorException("the scene class name is not fine");
        }

        public async UniTask ChangeSceneAsync(ESceneType sceneType, SceneContext context = default, bool sameScene = false)
        {
            if (!CurrentSceneType.EnumEquals(sceneType) || sameScene)
            {
                await FadeOutAsync();

                foreach (var kvp in SceneBaseDict) kvp.Value.OnDeleteScene();

                if (!sameScene)
                {
                    await AssetBundleManager.Instance.UnloadAssetsAsync(SceneBaseDict.AsValueEnumerable().Select(x => x.Key).ToList());
                    await Resources.UnloadUnusedAssets();
                }

                SceneBaseDict.Clear();

                await zenjectSceneLoader.LoadSceneAsync(sceneType.ToString(), extraBindings: container => container.Bind<SceneContext>().FromInstance(context).AsSingle()).ToUniTask();
                SceneBaseDict.GetValueOrDefault(sceneType)?.OnCreateScene();
                await UniTask.Yield();
                return;
            }
            Debug.LogWarning(ZString.Format("{0} is the same as before.", sceneType));
        }

        public async UniTask ChangeSceneAdditiveAsync(ESceneType sceneType, SceneContext context = default)
        {
            Loading.Instance.ShowLoading();
            if (SceneBaseDict.ContainsKey(sceneType))
            {
                Loading.Instance.HideLoading();
                return;
            }
            await zenjectSceneLoader.LoadSceneAsync(sceneType.ToString(), LoadSceneMode.Additive, container => container.Bind<SceneContext>().FromInstance(context).AsSingle()).ToUniTask();
            SceneBaseDict.GetValueOrDefault(sceneType)?.OnCreateScene();
            await UniTask.Yield();
        }
    }
}
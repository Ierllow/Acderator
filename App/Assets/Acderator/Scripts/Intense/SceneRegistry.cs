using Intense.Attribute;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Intense
{
    internal sealed class SceneRegistry
    {
        private readonly Dictionary<ESceneType, SceneBase> sceneBaseDict = new();

        public ESceneType CurrentSceneType { get; private set; }

        public bool Contains(ESceneType sceneType) => sceneBaseDict.ContainsKey(sceneType);

        public void DeleteAll()
        {
            foreach (var sceneBase in sceneBaseDict.Values) sceneBase.OnDeleteScene();
        }

        public void Clear()
        {
            sceneBaseDict.Clear();
            CurrentSceneType = default;
        }

        public void AddLoaded(ESceneType sceneType)
        {
            var loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneType.ToString());
            var sceneBase = !loadedScene.IsValid() || !loadedScene.isLoaded ? default : loadedScene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<SceneBase>(true)).FirstOrDefault(sceneBase => GetSceneType(sceneBase) == sceneType);
            if (sceneBase == null) return;

            var registeredSceneType = GetSceneType(sceneBase);
            if (registeredSceneType == ESceneType.None) throw new InvalidSceneTypeException(registeredSceneType);
            if (!sceneBaseDict.TryAdd(registeredSceneType, sceneBase)) throw new DuplicateSceneTypeException(registeredSceneType);
            CurrentSceneType = registeredSceneType;
        }

        public void Create(ESceneType sceneType) => sceneBaseDict.GetValueOrDefault(sceneType)?.OnCreateScene();

        private ESceneType GetSceneType(SceneBase sceneBase) => sceneBase.GetType().GetCustomAttribute<SceneTypeAttribute>()?.Type ?? ESceneType.None;
    }
}
using Intense.Attribute;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Intense
{
    internal sealed class SceneRegistry
    {
        private readonly Dictionary<SceneType, SceneBase> sceneBaseDict = new();

        public SceneType CurrentSceneType { get; private set; }

        public bool Contains(SceneType sceneType) => sceneBaseDict.ContainsKey(sceneType);

        public void DeleteAll()
        {
            foreach (var sceneBase in sceneBaseDict.Values) sceneBase.OnDeleteScene();
        }

        public void Clear()
        {
            sceneBaseDict.Clear();
            CurrentSceneType = default;
        }

        public void AddLoaded(SceneType sceneType)
        {
            var loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneType.ToString());
            var sceneBase = !loadedScene.IsValid() || !loadedScene.isLoaded ? default : loadedScene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<SceneBase>(true)).FirstOrDefault(sceneBase => GetSceneType(sceneBase) == sceneType);
            if (sceneBase == null) return;

            var registeredSceneType = GetSceneType(sceneBase);
            if (registeredSceneType == SceneType.None) throw new InvalidSceneTypeException(registeredSceneType);
            if (!sceneBaseDict.TryAdd(registeredSceneType, sceneBase)) throw new DuplicateSceneTypeException(registeredSceneType);
            CurrentSceneType = registeredSceneType;
        }

        public void Create(SceneType sceneType) => sceneBaseDict.GetValueOrDefault(sceneType)?.OnCreateScene();

        private SceneType GetSceneType(SceneBase sceneBase) => sceneBase.GetType().GetCustomAttribute<SceneTypeAttribute>()?.Type ?? SceneType.None;
    }
}
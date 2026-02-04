using System;
using System.Collections.Generic;

namespace Song
{
    public class SongControllerInitializer
    {
        public bool IsInit { get; private set; } = false;

        private readonly List<IController> controllerList = new();
        private readonly Dictionary<Type, object> dependencies = new();

        public void InitAll()
        {
            foreach (var controller in controllerList)
            {
                switch (controller)
                {
                    case IFingeController f:
                        f.Init();
                        break;
                    case ISongController s:
                        s.Init();
                        break;
                    case ITutorialController t when dependencies.TryGetValue(typeof(TutorialData), out var data):
                        t.Init((TutorialData)data);
                        break;
                    default:
                        break;
                }
            }
            IsInit = true;
        }

        public static SongControllerInitializerBuilder CreateBuilder() => new();

        public void Add<T>(T controller) where T : IController => controllerList.Add(controller);

        public void AddDependency<T>(T dependency) => dependencies.Add(typeof(T), dependency);
    }

    public class SongControllerInitializerBuilder
    {
        private readonly SongControllerInitializer initializer = new();

        public SongControllerInitializerBuilder WithDependency<T>(T dependency)
        {
            initializer.AddDependency(dependency);
            return this;
        }

        public SongControllerInitializerBuilder WithController<T>(T controller) where T : IController
        {
            initializer.Add(controller);
            return this;
        }

        public SongControllerInitializer Build() => initializer;
    }
}
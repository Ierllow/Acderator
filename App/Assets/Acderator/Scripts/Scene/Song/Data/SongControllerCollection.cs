using Cysharp.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using ZLinq;

namespace Song
{
    [Serializable]
    public sealed class SongControllerCollection : IInitializable
    {
        private readonly List<IController> controllerList = new();
        private readonly TutorialData tutorialData;

        public SongControllerCollection(List<IController> controllerList, TutorialData tutorialData) => (this.controllerList, this.tutorialData) = (controllerList, tutorialData);

        public void Initialize() => InitCore();

        private void InitCore()
        {
            var builder = SongControllerInitializer.CreateBuilder().WithDependency(tutorialData);
            controllerList.ForEach(c => builder.WithController(c));
            builder.Build().InitAll();
        }

        public bool TryGet<T>(out T controller) where T : class
        {
            controller = controllerList.AsValueEnumerable().OfType<T>().FirstOrDefault();
            return controller != default;
        }
    }
}
namespace Zenject
{
    public static partial class DiContainerExtensions
    {
        public static ConditionalDiContainer If(this DiContainer container, bool condition) => new(container, condition);
    }

    public sealed class ConditionalDiContainer
    {
        private readonly DiContainer container;
        private readonly bool enabled;

        public ConditionalDiContainer(DiContainer container, bool enabled)
        {
            this.container = container;
            this.enabled = enabled;
        }

        public ConditionalBinderGeneric<T> BindInterfacesAndSelfTo<T>() => enabled ? new(container.BindInterfacesAndSelfTo<T>(), true) : new(default, false);

        public sealed class ConditionalBinderGeneric<T>
        {
            private readonly FromBinderNonGeneric binder;
            private readonly bool enabled;

            public ConditionalBinderGeneric(FromBinderNonGeneric binder, bool enabled)
            {
                this.binder = binder;
                this.enabled = enabled;
            }

            public ConditionalBinderGeneric<T> FromComponentInHierarchy()
            {
                if (enabled)
                {
                    binder.FromComponentInHierarchy();
                }
                return this;
            }

            public ConditionalBinderGeneric<T> AsSingle()
            {
                if (enabled)
                {
                    binder.AsSingle();
                }
                return this;
            }
        }
    }
}
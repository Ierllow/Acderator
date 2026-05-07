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

        public ConditionalBinderNonGeneric BindInterfacesAndSelfTo<T>() => enabled ? new(container.BindInterfacesAndSelfTo<T>(), true) : new(default, false);

        public ConditionalBinderGeneric<T> Bind<T>() => enabled ? new(container.Bind<T>(), true) : new(default, false);
    }

    public sealed class ConditionalBinderNonGeneric
    {
        private readonly FromBinderNonGeneric binder;
        private readonly bool enabled;

        public ConditionalBinderNonGeneric(FromBinderNonGeneric binder, bool enabled)
        {
            this.binder = binder;
            this.enabled = enabled;
        }

        public ConditionalBinderNonGeneric FromComponentInHierarchy()
        {
            if (enabled)
            {
                binder.FromComponentInHierarchy();
            }
            return this;
        }

        public ConditionalBinderNonGeneric AsSingle()
        {
            if (enabled)
            {
                binder.AsSingle();
            }
            return this;
        }
    }

    public sealed class ConditionalBinderGeneric<T>
    {
        private readonly FromBinderGeneric<T> binder;
        private readonly bool enabled;

        public ConditionalBinderGeneric(FromBinderGeneric<T> binder, bool enabled)
        {
            this.binder = binder;
            this.enabled = enabled;
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
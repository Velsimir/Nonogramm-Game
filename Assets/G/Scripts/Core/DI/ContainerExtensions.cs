using MessagePipe;
using Zenject;

namespace G.Core.DI
{
    public static class ContainerExtensions
    {
        public static void BindService<TService>(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<TService>().AsSingle().NonLazy();
        }

        public static void BindServiceInterfacesOnly<TService>(this DiContainer container)
        {
            container.BindInterfacesTo<TService>().AsSingle().NonLazy();
        }

        public static void BindPresenter<TPresenter>(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<TPresenter>().AsSingle().NonLazy();
        }

        public static void InstallAllMessageBrokers(this DiContainer container, MessagePipeOptions options)
        {
            System.Type[] types = typeof(ContainerExtensions).Assembly.GetTypes();

            foreach (System.Type type in types)
            {
                if (type.IsAbstract == false || type.IsSealed == false)
                    continue;

                System.Reflection.MethodInfo method = type.GetMethod("Install",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

                if (method == null)
                    continue;

                System.Reflection.ParameterInfo[] parameters = method.GetParameters();

                if (parameters.Length != 2)
                    continue;

                if (parameters[0].ParameterType != typeof(DiContainer))
                    continue;

                if (parameters[1].ParameterType != typeof(MessagePipeOptions))
                    continue;

                method.Invoke(null, new object[] { container, options });
            }
        }
    }
}

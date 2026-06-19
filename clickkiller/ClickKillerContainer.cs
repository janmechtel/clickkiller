using Microsoft.Extensions.DependencyInjection;

namespace clickkiller
{
    public static class ClickKillerContainer
    {
        public static ServiceProvider ServiceProvider { get; private set; }

        public static void Initialize(ServiceCollection collection)
        {
            ServiceProvider = collection.BuildServiceProvider();
        }
    }
}

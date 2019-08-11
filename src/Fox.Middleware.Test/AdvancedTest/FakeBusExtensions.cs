namespace Fox.Middleware.Test.AdvancedTest
{
    using Contexts;
    using Fake3rdPartyFramework;
    using Microsoft.Extensions.DependencyInjection;

    public static class FakeBusExtensions
    {
        public static void Subscribe<T>(this FakeBus bus, string topic, ServiceProvider provider) where T : MessageBase
        {
            bus.Subscribe<T>(topic, msg =>
            {
                using (var scope = provider.CreateScope())
                {
                    var pipeline = scope.ServiceProvider.GetService<BusMiddleWare<T>>();
                    return pipeline.Execute(scope.ServiceProvider, msg);
                }
            });
        }
    }
}
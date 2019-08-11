namespace Fox.Middleware.Test.Contexts.Actions
{
    using System;
    using System.Threading.Tasks;
    using Infrastructure;

    public class TestAction1<T> : IAction<T>, IDisposable where T : MessageBase
    {
        private readonly Monitor _monitor;

        public TestAction1(Monitor monitor)
        {
            _monitor = monitor;
            _monitor.AddCtorTick(this);
        }

        public async Task Execute(T context, Next<T> next)
        {
            _monitor.AddCallTick(this);
            await next(context);
        }

        public void Dispose()
        {
            _monitor.AddDisposalTick(this);
        }
    }
}
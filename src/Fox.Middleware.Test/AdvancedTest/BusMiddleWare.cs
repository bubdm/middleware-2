namespace Fox.Middleware.Test.AdvancedTest
{
    using System;
    using System.Threading.Tasks;
    using Contexts;
    using Contexts.Actions;

    public class BusMiddleWare<T> : IMiddleware<T> where T : MessageBase
    {
        private readonly Middleware<T> _internalMiddleware;

        public BusMiddleWare()
        {
            _internalMiddleware = new Middleware<T>();
            _internalMiddleware.Add<TestAction1<T>>();
            _internalMiddleware.Add<TestGenericAction<T>>();
            _internalMiddleware.Add<ConsumerAction<T>>();
        }

        public async Task Execute(IServiceProvider scope, T context)
        {
            await _internalMiddleware.Execute(scope, context);
        }
    }
}
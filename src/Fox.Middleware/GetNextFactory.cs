namespace Fox.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// this is how we create the <see cref="Next{T}"/> delegate
    /// </summary>
    /// <typeparam name="TContext">the context this factory is for</typeparam>
    internal class GetNextFactory<TContext>
    {
        private readonly IEnumerator<PipeItem> _enumerator;
        private readonly IServiceProvider _scope;

        public GetNextFactory(IEnumerator<PipeItem> enumerator, IServiceProvider scope)
        {
            _enumerator = enumerator;
            _scope = scope;
        }


        public Next<TContext> GetNext()
        {
            if (!_enumerator.MoveNext())
            {
                return ctx => Task.CompletedTask;
            }

            var pipedType = _enumerator.Current;

            Task Next(TContext ctx)
            {
                IAction<TContext> middleware = null;

                if (pipedType.PipeItemType == PipeItemType.Type)
                {
                    middleware = (IAction<TContext>) _scope.GetService(pipedType.Type);
                }
                else
                {
                    middleware = (IAction<TContext>) pipedType.GivenInstance;
                }

                //note this is key, we call GetNext
                return middleware.Execute(ctx, GetNext());
            }

            return Next;

        }
    }
}
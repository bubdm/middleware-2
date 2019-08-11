namespace Fox.Middleware.Test.Contexts
{
    using System.Threading.Tasks;

    public interface IConsumer<in T>
    {
        Task Handle(T message);
    }
}
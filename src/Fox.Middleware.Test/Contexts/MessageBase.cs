namespace Fox.Middleware.Test.Contexts
{
    using System;

    public abstract class MessageBase
    {
        public Guid Id => Guid.NewGuid();
        public DateTime DateTime => DateTime.UtcNow;
    }
}
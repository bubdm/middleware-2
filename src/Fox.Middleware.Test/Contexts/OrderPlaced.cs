namespace Fox.Middleware.Test.Contexts
{
    public class OrderPlaced : MessageBase
    {
        public decimal Total { get; set; }
        public string CustomerId { get; set; }
    }
}
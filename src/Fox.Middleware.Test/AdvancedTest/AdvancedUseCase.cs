namespace Fox.Middleware.Test.AdvancedTest
{
    using System.Linq;
    using Contexts;
    using Contexts.Actions;
    using Fake3rdPartyFramework;
    using Infrastructure;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    /// <summary>
    /// this test is abit over the top, but it does represent the full intended usecase.
    /// </summary>
    public class AdvancedUseCase
    {
        ServiceProvider _baseScope;
        Monitor _monitor;
        Injected _injected;
        FakeBus _bus;

        [SetUp]
        public void Setup()
        {
            _monitor = new Monitor();
            _injected = new Injected();
            var collection = new ServiceCollection();

            //this registration is key, this is where the actions are associated with a middleware implementation.
            collection.AddSingleton(typeof(BusMiddleWare<>), typeof(BusMiddleWare<>));

            //register the Actions.
            collection.AddTransient(typeof(ConsumerAction<>));
            collection.AddSingleton(typeof(TestAction1<>));
            collection.AddTransient(typeof(TestGenericAction<>));

            //the framework we want to add middleware support with.
            collection.AddSingleton<FakeBus>();

            collection.AddSingleton(_monitor);
            collection.AddSingleton(_injected);


            //find all the IConsumer<T> interfaces
            var consumers = this.GetType().Assembly.ExportedTypes.SelectMany(x =>
            {
                var interfaces = x.GetInterfaces()
                    .Where(interfaceType => interfaceType.IsGenericType)
                    .Where(interfaceType => typeof(IConsumer<>).IsAssignableFrom(interfaceType.GetGenericTypeDefinition()))
                    .Select(interfaceType => new
                    {
                        Type = x,
                        Interface = interfaceType,
                        ConsumedType = interfaceType.GenericTypeArguments[0]
                    });

                return interfaces;
            });


            foreach (var consumer in consumers)
            {
                //Register IConsumer<T>
                collection.AddTransient(consumer.Interface, consumer.Type);
            }

            _baseScope = collection.BuildServiceProvider();
            _bus = _baseScope.GetService<FakeBus>();

            _bus.Subscribe<OrderPlaced>("OrderPlaced", _baseScope);
            _bus.Subscribe<OrderPaymentTaken>("OrderPaymentTaken", _baseScope);
        }

        [TearDown]
        public void TearDown()
        {
            _baseScope?.Dispose();
        }


        [Test]
        public void SingleCall()
        {
            _bus.Handle("OrderPlaced", new OrderPlaced()
            {
                CustomerId = "asd"
            }).Wait();

            Assert.AreEqual("asd", _injected.Value);
        }

        [Test]
        public void TwoCallsToTheSameDestinationPipeline()
        {

            _bus.Handle("OrderPlaced", new OrderPlaced()
            {
                CustomerId = "asd"
            }).Wait();

            Assert.AreEqual("asd", _injected.Value);

            _bus.Handle("OrderPlaced", new OrderPlaced()
            {
                CustomerId = "asd2"
            }).Wait();

            Assert.AreEqual("asd2", _injected.Value);
            Assert.AreEqual(typeof(OrderPlaced), _injected.MiddlewareOn);
            Assert.AreEqual(2, _monitor.NumberOfCtorTicksFor<TestGenericAction<OrderPlaced>>());
            Assert.AreEqual(2, _monitor.NumberOfDisposalTicksFor<TestGenericAction<OrderPlaced>>());

            Assert.AreEqual(1, _monitor.NumberOfCtorTicksFor<TestAction1<OrderPlaced>>());
            Assert.AreEqual(0, _monitor.NumberOfDisposalTicksFor<TestAction1<OrderPlaced>>());

        }


        [Test]
        public void CallOnDifferentPipelines()
        {

            _bus.Handle("OrderPlaced", new OrderPlaced()
            {
                CustomerId = "asd"
            }).Wait();

            Assert.AreEqual("asd", _injected.Value);
            Assert.AreEqual(typeof(OrderPlaced), _injected.MiddlewareOn);

            _bus.Handle("OrderPaymentTaken", new OrderPaymentTaken()
            {
                CustomerId = "asd2"
            }).Wait();

            Assert.AreEqual("asd2", _injected.Value);
            Assert.AreEqual(typeof(OrderPaymentTaken), _injected.MiddlewareOn);
            Assert.AreEqual(1, _monitor.NumberOfCtorTicksFor<TestGenericAction<OrderPlaced>>());
            Assert.AreEqual(1, _monitor.NumberOfDisposalTicksFor<TestGenericAction<OrderPlaced>>());

            Assert.AreEqual(1, _monitor.NumberOfCtorTicksFor<TestGenericAction<OrderPaymentTaken>>());
            Assert.AreEqual(1, _monitor.NumberOfDisposalTicksFor<TestGenericAction<OrderPaymentTaken>>());
        }

    }
}
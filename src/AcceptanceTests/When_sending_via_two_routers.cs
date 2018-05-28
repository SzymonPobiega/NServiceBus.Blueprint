namespace AcceptanceTests
{
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.Router;
    using NUnit.Framework;

    [TestFixture]
    public class When_sending_via_two_routers : NServiceBusAcceptanceTest
    {
        static string routingFilePath = $"{nameof(When_sending_via_two_routers)}.xml";

        [Test]
        public async Task Should_deliver_the_reply()
        {

            var result = await Scenario.Define<Context>()
                .WithRouter("Green-Blue", cfg =>
                {
                    cfg.AddInterface<TestTransport>("Green", t => t.BrokerAlpha()).InMemorySubscriptions();
                    cfg.AddInterface<TestTransport>("Blue", t => t.BrokerBravo()).InMemorySubscriptions();

                    var mapConfig = cfg.UseMapBasedRoutingProtocol<UrlXmlBlueprintAccessConfiguration>();
                    mapConfig.FilePath(routingFilePath);
                })
                .WithRouter("Red-Blue", cfg =>
                {
                    cfg.AddInterface<TestTransport>("Blue", t => t.BrokerBravo()).InMemorySubscriptions();
                    cfg.AddInterface<TestTransport>("Red", t => t.BrokerCharlie()).InMemorySubscriptions();

                    var mapConfig = cfg.UseMapBasedRoutingProtocol<UrlXmlBlueprintAccessConfiguration>();
                    mapConfig.FilePath(routingFilePath);
                })
                .WithEndpoint<Sender>(c => c.When(s => s.Send(new MyRequest())))
                .WithEndpoint<Receiver>()
                .Done(c => c.RequestReceived && c.ResponseReceived)
                .Run();

            Assert.IsTrue(result.RequestReceived);
            Assert.IsTrue(result.ResponseReceived);
        }

        class Context : ScenarioContext
        {
            public bool RequestReceived { get; set; }
            public bool ResponseReceived { get; set; }
        }

        class Sender : EndpointConfigurationBuilder
        {
            public Sender()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    c.UseTransport<TestTransport>().BrokerAlpha();
                    var mapConfig = c.UseBlueprintBasedRouting<UrlXmlBlueprintAccessConfiguration>();
                    mapConfig.FilePath(routingFilePath);
                });
            }

            class MyResponseHandler : IHandleMessages<MyResponse>
            {
                Context scenarioContext;

                public MyResponseHandler(Context scenarioContext)
                {
                    this.scenarioContext = scenarioContext;
                }

                public Task Handle(MyResponse response, IMessageHandlerContext context)
                {
                    scenarioContext.ResponseReceived = true;
                    return Task.CompletedTask;
                }
            }
        }

        class Receiver : EndpointConfigurationBuilder
        {
            public Receiver()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    //No bridge configuration needed for reply
                    c.UseTransport<TestTransport>().BrokerCharlie();
                });
            }

            class MyRequestHandler : IHandleMessages<MyRequest>
            {
                Context scenarioContext;

                public MyRequestHandler(Context scenarioContext)
                {
                    this.scenarioContext = scenarioContext;
                }

                public Task Handle(MyRequest request, IMessageHandlerContext context)
                {
                    scenarioContext.RequestReceived = true;
                    return context.Reply(new MyResponse());
                }
            }
        }

        class MyRequest : IMessage
        {
        }

        class MyResponse : IMessage
        {
        }
    }
}

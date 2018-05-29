namespace AcceptanceTests
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.Router;
    using NUnit.Framework;

    [TestFixture]
    public class When_two_routes_exist : NServiceBusAcceptanceTest
    {
        static string routingFilePath = $"{nameof(When_two_routes_exist)}.xml";

        [Test]
        public async Task Should_use_the_shortest_route()
        {

            var result = await Scenario.Define<Context>()
                .WithRouter("Green-Blue", cfg =>
                {
                    cfg.AddInterface<TestTransport>("Green", t => t.BrokerAlpha()).InMemorySubscriptions();
                    cfg.AddInterface<TestTransport>("Blue", t => t.BrokerBravo()).InMemorySubscriptions();

                    var mapConfig = cfg.UseMapBasedRoutingProtocol<UrlXmlBlueprintAccessConfiguration>();
                    mapConfig.FilePath(routingFilePath);
                })
                .WithRouter("Red-Blue", (ctx, cfg) =>
                {
                    cfg.AddInterface<TestTransport>("Blue", t => t.BrokerBravo()).InMemorySubscriptions();
                    cfg.AddInterface<TestTransport>("Red", t => t.BrokerDelta()).InMemorySubscriptions();

                    cfg.InterceptForwarding((queue, message, dispatch, forward) =>
                    {
                        ctx.LongRouteUsed = true;
                        return forward(dispatch);
                    });

                    var mapConfig = cfg.UseMapBasedRoutingProtocol<UrlXmlBlueprintAccessConfiguration>();
                    mapConfig.FilePath(routingFilePath);
                })
                .WithRouter("Green-Red", (ctx, cfg) =>
                {
                    cfg.AddInterface<TestTransport>("Green", t => t.BrokerAlpha()).InMemorySubscriptions();
                    cfg.AddInterface<TestTransport>("Red", t => t.BrokerDelta()).InMemorySubscriptions();

                    cfg.InterceptForwarding((queue, message, dispatch, forward) =>
                    {
                        ctx.ShortRouteUsed = true;
                        return forward(dispatch);
                    });

                    var mapConfig = cfg.UseMapBasedRoutingProtocol<UrlXmlBlueprintAccessConfiguration>();
                    mapConfig.FilePath(routingFilePath);
                })
                .WithEndpoint<Sender>(c => c.When(s => s.Send(new MyRequest())))
                .WithEndpoint<Receiver>()
                .Done(c => c.RequestReceived && c.ResponseReceived)
                .Run(TimeSpan.FromSeconds(30));

            Assert.IsTrue(result.RequestReceived);
            Assert.IsTrue(result.ResponseReceived);
            Assert.IsTrue(result.ShortRouteUsed);
            Assert.IsFalse(result.LongRouteUsed);
        }

        class Context : ScenarioContext
        {
            public bool RequestReceived { get; set; }
            public bool ResponseReceived { get; set; }
            public bool ShortRouteUsed { get; set; }
            public bool LongRouteUsed { get; set; }
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
                    c.UseTransport<TestTransport>().BrokerDelta();
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

using System.Threading.Tasks;
using NServiceBus.Features;
using NServiceBus.Routing;
using NServiceBus.Transport;

// Features are always defined in NServiceBus namespace
// ReSharper disable once CheckNamespace
namespace NServiceBus
{
    using System;
    using Blueprint;
    using Logging;

    class BlueprintBasedRoutingFeature : Feature
    {
        protected override void Setup(FeatureConfigurationContext context)
        {
            var transportInfrastructure = context.Settings.Get<TransportInfrastructure>();
            var routingEngineConfig = context.Settings.Get<BlueprintAccessConfiguration>();
            var routingEngine = routingEngineConfig.Create();

            var outboundRoutingPolicy = context.Settings.Get<TransportInfrastructure>().OutboundRoutingPolicy;

            var evaluator = new BlueprintBasedRouteGenerator(context.Settings.EndpointName());
            var router = new UnicastRouter(evaluator, 
                context.Settings.Get<EndpointInstances>(), 
                context.Settings.Get<DistributionPolicy>(),
                instance => transportInfrastructure.ToTransportAddress(LogicalAddress.CreateRemoteAddress(instance)));

            context.RegisterStartupTask(new EngineManagementTask(routingEngine, map => evaluator.UpdateSourceData(map)));

            context.Pipeline.Register(new RouteToSitesBehavior(), "Adds site information to outgoing messages.");

            // if the transport provides native pub/sub support, don't plug in the FileBased pub/sub storage.
            if (outboundRoutingPolicy.Publishes == OutboundRoutingType.Unicast)
            {
                context.Pipeline.Replace("UnicastPublishRouterConnector", new PublishRoutingConnector(router));
                context.Pipeline.Replace("MessageDrivenSubscribeTerminator", new NullSubscribeTerminator(), "handles subscribe operations");
                context.Pipeline.Replace("MessageDrivenUnsubscribeTerminator", new NullUnsubscribeTerminator(), "handles ubsubscribe operations");
            }
            if (outboundRoutingPolicy.Sends == OutboundRoutingType.Unicast)
            {
                context.Pipeline.Replace("UnicastSendRouterConnector", new SendRoutingConnector(router));
            }
            else
            {
                context.Pipeline.Replace("UnicastSendRouterConnector", new MulticastSendRoutingConnector());
            }
        }

        class EngineManagementTask : FeatureStartupTask
        {
            static ILog logger = LogManager.GetLogger<BlueprintBasedRoutingFeature>();

            IBlueprintAccess engine;
            Action<Blueprint.Blueprint> topologyChanged;

            public EngineManagementTask(IBlueprintAccess engine, Action<Blueprint.Blueprint> topologyChanged)
            {
                this.engine = engine;
                this.topologyChanged = topologyChanged;
            }

            protected override Task OnStart(IMessageSession session)
            {
                return engine.Start(topologyChanged, exception => { logger.Error("Error while refreshing system map.", exception); });
            }

            protected override Task OnStop(IMessageSession session)
            {
                return engine.Stop();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using NServiceBus;
using NServiceBus.Pipeline;
using NServiceBus.Routing;

class UnicastRouter
{
    BlueprintBasedRouteGenerator routeGenerator;
    EndpointInstances endpointInstances;
    IDistributionPolicy distributionPolicy;
    Func<EndpointInstance, string> resolveTransportAddress;

    public UnicastRouter(BlueprintBasedRouteGenerator routeGenerator, EndpointInstances endpointInstances, IDistributionPolicy distributionPolicy, Func<EndpointInstance, string> resolveTransportAddress)
    {
        this.routeGenerator = routeGenerator;
        this.endpointInstances = endpointInstances;
        this.distributionPolicy = distributionPolicy;
        this.resolveTransportAddress = resolveTransportAddress;
    }

    public IEnumerable<RoutingStrategy> Route(Type messageType, IOutgoingContext context, OutgoingLogicalMessage outgoingMessage, DistributionStrategyScope distributionStrategyScope)
    {
        var routes = routeGenerator.GetRoutesFor(messageType);

        var matchingRoutes = routes.Where(r => r.Match(GetDestinationSites(context)));

        foreach (var destination in matchingRoutes)
        {
            var candidates = ResolveRoute(destination).ToArray();
            var distributionContext = new DistributionContext(candidates, outgoingMessage, context.MessageId, context.Headers, resolveTransportAddress, context.Extensions);
            var distributionStrategy = distributionPolicy.GetDistributionStrategy(destination.ImmediateDestination, distributionStrategyScope);
            var selected = distributionStrategy.SelectDestination(distributionContext);

            var routingStrategy = new MapBasedRoutingStrategy(selected, destination.NextHop);
            yield return routingStrategy;
        }
    }

    static string[] GetDestinationSites(IOutgoingContext context)
    {
        if (!context.Extensions.TryGet<RouteToSitesBehavior.State>(out var state))
        {
            return new string[0];
        }

        return state.Sites;
    }

    IEnumerable<string> ResolveRoute(Route destination)
    {
        foreach (var instance in endpointInstances.FindInstances(destination.ImmediateDestination))
        {
            yield return resolveTransportAddress(instance);
        }
    }

    class MapBasedRoutingStrategy : RoutingStrategy
    {
        string immediateDestination;
        string nextHop;

        public MapBasedRoutingStrategy(string immediateDestination, string nextHop)
        {
            this.immediateDestination = immediateDestination;
            this.nextHop = nextHop;
        }

        public override AddressTag Apply(Dictionary<string, string> headers)
        {
            if (nextHop != null)
            {
                headers["NServiceBus.Bridge.DestinationEndpoint"] = nextHop;
            }
            return new UnicastAddressTag(immediateDestination);
        }
    }
}
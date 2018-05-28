using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NServiceBus.Blueprint;

class BlueprintBasedRouteGenerator
{
    SourceData sourceData;
    ConcurrentDictionary<Type, IReadOnlyCollection<Route>> cachedRoutes = new ConcurrentDictionary<Type, IReadOnlyCollection<Route>>();
    string endpointName;

    public BlueprintBasedRouteGenerator(string endpointName)
    {
        this.endpointName = endpointName;
    }

    public IReadOnlyCollection<Route> GetRoutesFor(Type messageType)
    {
        return cachedRoutes.GetOrAdd(messageType, type => CalcuateRoute(type, sourceData));
    }

    static IReadOnlyCollection<Route> CalcuateRoute(Type messageType, SourceData sourceData)
    {
        //TODO: Handle same endpoint in multiple sites

        var matchingDestinations = sourceData.SystemMap.Destinations.Where(g => g.Selector(new MessageType(messageType)));

        IEnumerable<Route> RoutesToDestination(string d)
        {
            if (sourceData.LocalSite.Endpoints.Contains(d))
            {
                yield return new Route(d, null, null);
                yield break;
            }

            if (sourceData.DestinationToGateway.TryGetValue(d, out var gatewaySet))
            {
                if (gatewaySet.Count == 1) //Single site
                {
                    yield return new Route(gatewaySet.First().Value.Name, d, null);
                }
                else
                {
                    foreach (var pair in gatewaySet)
                    {
                        yield return new Route(pair.Value.Name, d, pair.Key);
                    }
                }
            }
        }

        return matchingDestinations.SelectMany(x => x.Destinations.SelectMany(RoutesToDestination)).ToArray();
    }

    static Dictionary<string, Dictionary<string, GatewayInfo>> MapDestinationToGateway(string endpointName, Blueprint systemMap)
    {
        var localSite = systemMap.Sites.First(s => s.Endpoints.Contains(endpointName));

        var result = new Dictionary<string, Dictionary<string, GatewayInfo>>();

        foreach (var nearbyGateway in localSite.Routers)
        {
            var destinations = FindDestinations(nearbyGateway, systemMap, new List<Site> {localSite});
            foreach (var destination in destinations)
            {
                if (!result.TryGetValue(destination.Endpoint, out var endpointsGateways))
                {
                    endpointsGateways = new Dictionary<string, GatewayInfo>();
                    result[destination.Endpoint] = endpointsGateways;
                }

                if (!endpointsGateways.TryGetValue(destination.Site, out var gatewayInfo) || gatewayInfo.Distance > destination.Distance)
                {
                    endpointsGateways[destination.Site] = new GatewayInfo(nearbyGateway.Name, destination.Distance);
                }
            }
        }

        return result;
    }

    static IEnumerable<Destination> FindDestinations(Router root, Blueprint map, List<Site> visitedSites, int distance = 0)
    {
        var connectedSites = map.Sites.Where(s => s.Routers.Any(r => r.Name == root.Name));

        foreach (var site in connectedSites)
        {
            if (visitedSites.Contains(site))
            {
                continue;
            }
            visitedSites.Add(site);
            foreach (var endpoint in site.Endpoints)
            {
                yield return new Destination(endpoint, site.Name, distance);
            }
            var nearbyGateways = site.Routers.Where(r => r.Name != root.Name);
            foreach (var router in nearbyGateways)
            {
                var furtherDestinations = FindDestinations(router, map, visitedSites, distance + 1);
                foreach (var furtherDestination in furtherDestinations)
                {
                    yield return furtherDestination;
                }
            }
        }
    }

    public void UpdateSourceData(Blueprint newSystemMap)
    {
        var destinationToGateway = MapDestinationToGateway(endpointName, newSystemMap);
        var localSite = newSystemMap.Sites.First(s => s.Endpoints.Contains(endpointName));

        sourceData = new SourceData(newSystemMap, destinationToGateway, localSite);
        cachedRoutes.Clear();
    }

    class SourceData
    {
        public readonly Blueprint SystemMap;
        public readonly Dictionary<string, Dictionary<string, GatewayInfo>> DestinationToGateway;
        public readonly Site LocalSite;

        public SourceData(Blueprint systemMap, Dictionary<string, Dictionary<string, GatewayInfo>> destinationToGateway, Site localSite)
        {
            SystemMap = systemMap;
            DestinationToGateway = destinationToGateway;
            LocalSite = localSite;
        }
    }

    class GatewayInfo
    {
        public readonly string Name;
        public readonly int Distance;

        public GatewayInfo(string name, int distance)
        {
            Name = name;
            Distance = distance;
        }
    }

    class Destination
    {
        public readonly string Endpoint;
        public readonly string Site;
        public readonly int Distance;

        public Destination(string endpoint, string site, int distance)
        {
            Endpoint = endpoint;
            Site = site;
            Distance = distance;
        }
    }
}
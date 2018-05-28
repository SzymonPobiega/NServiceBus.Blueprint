using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus.Blueprint;
using NServiceBus.Logging;
using NServiceBus.Router;
using Router = NServiceBus.Blueprint.Router;

class BlueprintBasedRouting : IRoutingProtocol
{
    static ILog logger = LogManager.GetLogger<BlueprintBasedRouting>();

    BlueprintAccessConfiguration accessConfiguration;
    IBlueprintAccess blueprintAccess;

    public BlueprintBasedRouting(BlueprintAccessConfiguration accessConfiguration)
    {
        this.accessConfiguration = accessConfiguration;
    }

    public Task Start(RouterMetadata metadata)
    {
        blueprintAccess = accessConfiguration.Create();
        return blueprintAccess.Start(map =>
        {
            RouteTable = BuildRouteTable(metadata, map);
        }, ex => { logger.Error("Error while refreshing system map.", ex); });
    }

    internal static RouteTable BuildRouteTable(RouterMetadata metadata, Blueprint map)
    {
        logger.Debug($"Building routing table for {metadata.Name}.");

        var table = new RouteTable();

        var directlyConnectedSites = map.Sites.Where(s => s.Routers.Any(r => r.Name == metadata.Name)).ToList();

        var endpointCardinality = map.Sites.SelectMany(s => s.Endpoints, (site, endpoint) => new
        {
            site,
            endpoint
        }).GroupBy(x => x.endpoint).ToDictionary(g => g.Key, g => g.Count() > 1);

        foreach (var iface in metadata.Interfaces)
        {
            var site = FindSiteConnectedToInterface(map, metadata.Name, iface);

            foreach (var endpoint in site.Endpoints)
            {
                if (endpointCardinality[endpoint])
                {
                    table.AddRoute((i, d) => i != iface && d.Endpoint == endpoint && d.Site == site.Name, $"Interface <> {iface} AND Endpoint = {endpoint} AND Site = {site.Name}", null, iface);
                }
                else
                {
                    table.AddRoute((i, d) => i != iface && d.Endpoint == endpoint, $"Interface <> {iface} AND Endpoint = {endpoint}", null, iface);
                }
            }

            var nearbyGateways = site.Routers.Where(r => r.Name != metadata.Name);

            foreach (var gateway in nearbyGateways)
            {
                var destinations = FindDestinations(gateway, metadata, map, new List<Site>(directlyConnectedSites)).ToArray();
                foreach (var destination in destinations)
                {
                    if (endpointCardinality[destination.Endpoint])
                    {
                        table.AddRoute((i, d) => i != gateway.Interface && d.Endpoint == destination.Endpoint && d.Site == destination.Site, $"Interface <> {gateway.Interface} AND Endpoint = {destination.Endpoint} AND Site = {destination.Site}", gateway.Name, iface);
                    }
                    else
                    {
                        table.AddRoute((i, d) => i != gateway.Interface && d.Endpoint == destination.Endpoint, $"Interface <> {gateway.Interface} AND Endpoint = {destination.Endpoint}", gateway.Name, iface);
                    }
                }
            }
        }
        return table;
    }

    static IEnumerable<Destination> FindDestinations(Router root, RouterMetadata metadata, Blueprint map, List<Site> visitedSites)
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
                yield return new Destination(endpoint, site.Name);
            }
            var nearbyGateways = site.Routers.Where(r => r.Name != metadata.Name);
            foreach (var router in nearbyGateways)
            {
                var furtherDestinations = FindDestinations(router, metadata, map, visitedSites);
                foreach (var furtherDestination in furtherDestinations)
                {
                    yield return furtherDestination;
                }
            }
        }
    }

    static Site FindSiteConnectedToInterface(Blueprint map, string router, string iface)
    {
        return map.Sites.FirstOrDefault(s => s.Routers.Any(r => r.Name == router && r.Interface == iface));
    }

    public Task Stop()
    {
        return blueprintAccess.Stop();
    }

    public RouteTable RouteTable { get; private set; }

    class Destination
    {
        public readonly string Endpoint;
        public readonly string Site;

        public Destination(string endpoint, string site)
        {
            Endpoint = endpoint;
            Site = site;
        }
    }
}

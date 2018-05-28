namespace NServiceBus.Blueprint
{
    using System.Collections.Generic;

    public class Site
    {
        public Site(string name, List<string> endpoints, List<Router> routers)
        {
            Endpoints = endpoints;
            Routers = routers;
            Name = name;
        }

        public string Name { get; }
        public IReadOnlyCollection<string> Endpoints { get; }
        public IReadOnlyCollection<Router> Routers { get; }
    }
}
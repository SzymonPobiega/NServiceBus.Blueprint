namespace NServiceBus.Blueprint
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a site (e.g. a data centre) in a distributed NServiceBus-based system.
    /// </summary>
    public class Site
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="name">Name of the site.</param>
        /// <param name="endpoints">List of endpoints deployed to this site.</param>
        /// <param name="routers">List of routers deployed to this site. One router is, by definition, always deployed to more than one site.</param>
        public Site(string name, List<string> endpoints, List<Router> routers)
        {
            Endpoints = endpoints;
            Routers = routers;
            Name = name;
        }

        /// <summary>
        /// Gets the name of the site.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the list of endpoints deployed to this site.
        /// </summary>
        public IReadOnlyCollection<string> Endpoints { get; }

        /// <summary>
        /// Gets the list of routers deployed to this site.
        /// </summary>
        public IReadOnlyCollection<Router> Routers { get; }
    }
}
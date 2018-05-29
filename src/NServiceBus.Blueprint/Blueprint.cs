using System.Collections.Generic;

namespace NServiceBus.Blueprint
{
    /// <summary>
    /// Represents the blueprint of a multi-site (data centre) distributed NServiceBus system.
    /// </summary>
    public class Blueprint
    {
        /// <summary>
        /// Creates a new instance of the blueprint.
        /// </summary>
        public Blueprint(List<MessageDestinations> destinations, List<Site> sites)
        {
            Destinations = destinations;
            Sites = sites;
        }

        /// <summary>
        /// Mappings of message groups to endpoints groups
        /// </summary>
        public IReadOnlyCollection<MessageDestinations> Destinations { get; }

        /// <summary>
        /// Sites.
        /// </summary>
        public IReadOnlyCollection<Site> Sites { get; }
    }
}
using System.Collections.Generic;

namespace NServiceBus.Blueprint
{
    public class Blueprint
    {
        public Blueprint(List<MessageDestinations> destinations, List<Site> sites)
        {
            Destinations = destinations;
            Sites = sites;
        }

        public IReadOnlyCollection<MessageDestinations> Destinations { get; }
        public IReadOnlyCollection<Site> Sites { get; }
    }
}
using System;
using System.Collections.Generic;

namespace NServiceBus.Blueprint
{
    /// <summary>
    /// Mapping of a group of message types to a group of endpoints.
    /// </summary>
    public class MessageDestinations
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="selector">A predicate for matching message types to include in the group.</param>
        /// <param name="destinations">A list of destination endpoints.</param>
        public MessageDestinations(Func<MessageType, bool> selector, List<string> destinations)
        {
            Selector = selector;
            Destinations = destinations;
        }

        /// <summary>
        /// A predicate for matching message types to include in the group.
        /// </summary>
        public Func<MessageType, bool> Selector { get; }

        /// <summary>
        /// A list of destination endpoints.
        /// </summary>
        public IReadOnlyCollection<string> Destinations { get; }
    }
}
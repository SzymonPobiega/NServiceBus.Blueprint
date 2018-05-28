using System;
using System.Collections.Generic;

namespace NServiceBus.Blueprint
{
    public class MessageDestinations
    {
        public MessageDestinations(Func<MessageType, bool> selector, List<string> destinations)
        {
            Selector = selector;
            Destinations = destinations;
        }

        public Func<MessageType, bool> Selector { get; }
        public IReadOnlyCollection<string> Destinations { get; }
    }
}
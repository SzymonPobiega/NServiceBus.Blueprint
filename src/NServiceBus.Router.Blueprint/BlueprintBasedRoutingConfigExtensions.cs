namespace NServiceBus.Router
{
    using Blueprint;

    /// <summary>
    /// Configures the router to use blueprint-based routing protocol.
    /// </summary>
    public static class BlueprintBasedRoutingConfigExtensions
    {
        /// <summary>
        /// Configures the router to use blueprint-based routing protocol.
        /// </summary>
        public static T UseMapBasedRoutingProtocol<T>(this RouterConfiguration config)
            where T : BlueprintAccessConfiguration, new()
        {
            var mapConfig = new T();
            var protocol = new BlueprintBasedRouting(mapConfig);
            config.UseRoutingProtocol(protocol);
            return mapConfig;
        }
    }
}
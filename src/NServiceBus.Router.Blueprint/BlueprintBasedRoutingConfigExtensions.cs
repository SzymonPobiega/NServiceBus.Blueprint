namespace NServiceBus.Router
{
    using Blueprint;

    public static class BlueprintBasedRoutingConfigExtensions
    {
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
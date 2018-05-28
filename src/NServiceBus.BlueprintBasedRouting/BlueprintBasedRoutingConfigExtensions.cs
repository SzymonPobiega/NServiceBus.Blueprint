
namespace NServiceBus
{
    using Blueprint;
    using Configuration.AdvancedExtensibility;
    using Features;

    /// <summary>
    /// Extensions for configuring blueprint-based routing functionality.
    /// </summary>
    public static class BlueprintBasedRoutingConfigExtensions
    {
        /// <summary>
        /// Enables routing configured with the system map.
        /// </summary>
        /// <param name="config">The configuration object.</param>
        public static T UseBlueprintBasedRouting<T>(this EndpointConfiguration config)
            where T : BlueprintAccessConfiguration, new()
        {
            var engineConfig = config.GetSettings().GetOrCreate<T>();
            config.GetSettings().Set<BlueprintAccessConfiguration>(engineConfig);
            config.GetSettings().EnableFeatureByDefault<BlueprintBasedRoutingFeature>();
            return engineConfig;
        }
    }
}
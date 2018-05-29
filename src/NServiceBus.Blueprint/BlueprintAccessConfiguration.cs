namespace NServiceBus.Blueprint
{
    /// <summary>
    /// Allow creating blueprint access objects based on a specific technology and configuration.
    /// </summary>
    public abstract class BlueprintAccessConfiguration
    {
        /// <summary>
        /// Creates the blueprint access.
        /// </summary>
        public abstract IBlueprintAccess Create();
    }
}
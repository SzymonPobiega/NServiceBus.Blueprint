namespace NServiceBus.Blueprint
{
    /// <summary>
    /// Represents the router process within a given site.
    /// </summary>
    public class Router
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public Router(string name, string @interface)
        {
            Name = name;
            Interface = @interface;
        }

        /// <summary>
        /// Gets the name of the router.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the name of the router interface attached to the site.
        /// </summary>
        public string Interface { get; }
    }
}

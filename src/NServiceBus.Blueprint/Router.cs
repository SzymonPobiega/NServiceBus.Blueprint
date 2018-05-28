namespace NServiceBus.Blueprint
{
    public class Router
    {
        public Router(string name, string @interface)
        {
            Name = name;
            Interface = @interface;
        }

        public string Name { get; }
        public string Interface { get; }
    }
}

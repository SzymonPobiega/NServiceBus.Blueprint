namespace NServiceBus
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Pipeline;

    class RouteToSitesBehavior : Behavior<IRoutingContext>
    {
        public override Task Invoke(IRoutingContext context, Func<Task> next)
        {
            if (!context.Extensions.TryGet<State>(out var state))
            {
                return next();
            }
            if (state.Sites.Any(s => s.Contains(";")))
            {
                throw new Exception("Site name cannot contain a semicolon.");
            }
            context.Message.Headers["NServiceBus.Bridge.DestinationSites"] = string.Join(";", state.Sites);
            return next();
        }

        public class State
        {
            public string[] Sites { get; set; }
        }
    }
}
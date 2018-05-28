using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Pipeline;

class NullUnsubscribeTerminator : PipelineTerminator<IUnsubscribeContext>
{
    protected override Task Terminate(IUnsubscribeContext context)
    {
        log.Debug($"Unsubscribe was called for {context.EventType.FullName}. With FileBasedRouting, unsubscribe operations have no effect and subscribers should be configured in the routing file.");
        return Task.FromResult(0);
    }

    static readonly ILog log = LogManager.GetLogger<BlueprintBasedRoutingFeature>();
}

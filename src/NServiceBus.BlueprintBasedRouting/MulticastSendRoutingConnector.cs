using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Pipeline;
using NServiceBus.Routing;

class MulticastSendRoutingConnector : StageConnector<IOutgoingSendContext, IOutgoingLogicalMessageContext>
{
    public override Task Invoke(IOutgoingSendContext context, Func<IOutgoingLogicalMessageContext, Task> stage)
    {
        context.Headers[Headers.MessageIntent] = MessageIntentEnum.Send.ToString();

        var logicalMessageContext = this.CreateOutgoingLogicalMessageContext(
            context.Message,
            new[]
            {
                new MulticastRoutingStrategy(context.Message.MessageType)
            },
            context);

        return stage(logicalMessageContext);
    }
}
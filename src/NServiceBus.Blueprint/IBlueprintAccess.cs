using System;
using System.Threading.Tasks;

namespace NServiceBus.Blueprint
{
    public interface IBlueprintAccess
    {
        Task Start(Action<Blueprint> onChanged, Action<Exception> onError);
        Task Stop();
    }
}
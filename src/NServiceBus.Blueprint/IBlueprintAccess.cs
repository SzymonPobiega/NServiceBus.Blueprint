using System;
using System.Threading.Tasks;

namespace NServiceBus.Blueprint
{
    /// <summary>
    /// Allows to access the blueprint and notifies about changes in the blueprint while the system is running.
    /// </summary>
    public interface IBlueprintAccess
    {
        /// <summary>
        /// Initializes the access object.
        /// </summary>
        /// <param name="onChanged">Callback to invoke when the blueprint changes.</param>
        /// <param name="onError">Callback to invoke when the blueprint cannot be accessed.</param>
        Task Start(Action<Blueprint> onChanged, Action<Exception> onError);

        /// <summary>
        /// Stops sending blueprint changed notifications.
        /// </summary>
        Task Stop();
    }
}
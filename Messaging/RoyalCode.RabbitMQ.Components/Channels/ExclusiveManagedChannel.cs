using Microsoft.Extensions.Logging;
using RoyalCode.RabbitMQ.Components.Connections;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <inheritdoc />
internal sealed class ExclusiveManagedChannel : ManagedChannel
{
    public ExclusiveManagedChannel(ManagedConnection managedConnection, ILogger logger) 
        : base(managedConnection, logger)
    { }

    /// <inheritdoc />
    protected override bool ReleaseChannel()
    {
        logger.LogDebug("Release the exclusive channel called, close the channel.");
        return true;
    }
}

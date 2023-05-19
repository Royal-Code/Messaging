using Microsoft.Extensions.Logging;
using RoyalCode.RabbitMQ.Components.Connections;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <inheritdoc />
internal sealed class SharedManagedChannel : ManagedChannel
{
    private bool disposing;

    public SharedManagedChannel(ManagedConnection managedConnection, ILogger logger)
        : base(managedConnection, logger)
    { }

    internal void Terminate()
    {
        disposing = true;
        Dispose();
    }

    /// <inheritdoc />
    protected override bool ReleaseChannel()
    {
        if (disposing)
            logger.LogInformation("Disposing the shared channel.");
        else
            logger.LogDebug("Release the shared channel called, nothing to do.");

        return disposing;
    }
}

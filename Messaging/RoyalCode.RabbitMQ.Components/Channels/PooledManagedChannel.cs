using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using RoyalCode.RabbitMQ.Components.Connections;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <inheritdoc />
internal sealed class PooledManagedChannel : ManagedChannel
{
    private readonly ObjectPool<PooledManagedChannel> pool;

    public PooledManagedChannel(
        ManagedConnection managedConnection,
        ILogger logger,
        ObjectPool<PooledManagedChannel> pool)
        : base(managedConnection, logger)
    {
        this.pool = pool;
    }

    /// <inheritdoc />
    protected override bool ReleaseChannel()
    {
        logger.LogDebug("Releasing the pooled channel");

        pool.Return(this);

        return false;
    }
}

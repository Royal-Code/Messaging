using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using RoyalCode.RabbitMQ.Components.Connections;
using RoyalCode.RabbitMQ.Components.ObjectPool;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// Channel pool policy and provider of pooled channels.
/// </summary>
internal sealed class ChannelPool : IPooledObjectPolicy<PooledManagedChannel>, IDisposable
{
    private readonly DisposableObjectPool<PooledManagedChannel> pool;
    private readonly ManagedConnection managedConnection;
    private readonly ILoggerFactory loggerFactory;
    private readonly string clusterName;
    private readonly ILogger logger;

    /// <summary>
    /// Creates a new channel pool.
    /// </summary>
    /// <param name="managedConnection">The managed connection.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="maximumRetained">The maximum number of objects to retain in the pool.</param>
    /// <param name="clusterName">The name of the RabbitMQ cluster.</param>
    /// <exception cref="ArgumentNullException">
    ///     If some parameter was <see langword="null"/>.
    /// </exception>
    public ChannelPool(
        ManagedConnection managedConnection,
        ILoggerFactory loggerFactory,
        int maximumRetained,
        string clusterName)
    {
        this.managedConnection = managedConnection ?? throw new ArgumentNullException(nameof(managedConnection));
        this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        this.clusterName = clusterName;

        pool = new DisposableObjectPool<PooledManagedChannel>(this, maximumRetained);
        logger = loggerFactory.CreateLogger<ChannelPool>();
    }

    /// <summary>
    /// Gets the object from the pool.
    /// </summary>
    /// <returns>A pooled channel.</returns>
    public PooledManagedChannel GetManagedChannel()
    {
        return pool.Get();
    }

    /// <inheritdoc />
    public PooledManagedChannel Create()
    {
        logger.LogInformation("Creating a new pooled managed channel for the RabbitMQ cluster {ClusterName}", clusterName);

        var channel = new PooledManagedChannel(
            managedConnection,
            loggerFactory.CreateLogger<PooledManagedChannel>(),
            pool);

        return channel;
    }

    /// <inheritdoc />
    public bool Return(PooledManagedChannel channel)
    {
        logger.LogInformation("Returning a pooled managed channel for the RabbitMQ cluster {ClusterName}", clusterName);
        return true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        pool.Dispose();
    }
}
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RoyalCode.RabbitMQ.Components.Connections;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <inheritdoc />
public sealed class ChannelManager : IChannelManager, IDisposable
{
    private readonly ManagedConnection managedConnection;
    private readonly ILoggerFactory loggerFactory;
    private readonly ChannelPool channelPool;
    private readonly ILogger logger;

    private SharedManagedChannel? sharedChannel;
    private bool disposed;

    /// <summary>
    /// Creates a new channel manager.
    /// </summary>
    /// <param name="clusterName">The name of the RabbitMQ cluster.</param>
    /// <param name="managedConnection">A managed connection.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="optionsMonitor">Options for pooled channels.</param>
    public ChannelManager(
        string clusterName,
        ManagedConnection managedConnection,
        ILoggerFactory loggerFactory,
        IOptionsMonitor<ChannelPoolOptions> optionsMonitor)
    {
        ClusterName = clusterName;

        this.managedConnection = managedConnection;
        this.loggerFactory = loggerFactory;

        logger = loggerFactory.CreateLogger<ChannelManager>();
        channelPool = new ChannelPool(
            managedConnection,
            loggerFactory,
            optionsMonitor.Get(clusterName).PoolMaxSize,
            clusterName);
    }

    /// <inheritdoc />
    public string ClusterName { get; }

    /// <inheritdoc />
    public ManagedChannel CreateChannel()
    {
        logger.LogInformation("Creating channel manager for the RabbitMQ cluster {ClusterName}", ClusterName);

        var managedChannel = new ExclusiveManagedChannel(
            managedConnection,
            loggerFactory.CreateLogger<ExclusiveManagedChannel>());

        return managedChannel;
    }

    /// <inheritdoc />
    public ManagedChannel GetPooledChannel() => channelPool.GetManagedChannel();

    /// <inheritdoc />
    public ManagedChannel GetSharedChannel() => sharedChannel ??= SafeCreateSharedChannel();

    private SharedManagedChannel SafeCreateSharedChannel()
    {
        lock(logger)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(ChannelManager));

            if (sharedChannel is not null)
                return sharedChannel;
            
            logger.LogInformation("Creating shared channel manager for the RabbitMQ cluster {ClusterName}", ClusterName);
            
            var managedChannel = new SharedManagedChannel(
                managedConnection,
                loggerFactory.CreateLogger<SharedManagedChannel>());

            return managedChannel;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;

        logger.LogDebug("Disposing channel manager for the RabbitMQ cluster {ClusterName}", ClusterName);

        sharedChannel?.Terminate();
        sharedChannel = null;

        channelPool?.Dispose();
    }
}




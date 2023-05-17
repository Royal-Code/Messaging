using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RoyalCode.RabbitMQ.Components.Connections;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <inheritdoc />
public sealed class ChannelManager : IChannelManager
{
    private readonly ConnectionManager connectionManager;
    private readonly ILoggerFactory loggerFactory;
    private readonly IOptionsMonitor<ChannelPoolOptions> optionsMonitor;
    private readonly ILogger logger;

    private ManagedChannel? sharedChannel;

    /// <summary>
    /// Creates a new channel manager.
    /// </summary>
    /// <param name="connectionManager">The connection manager.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="optionsMonitor">Options for pooled channels.</param>
    public ChannelManager(
        ConnectionManager connectionManager,
        ILoggerFactory loggerFactory,
        IOptionsMonitor<ChannelPoolOptions> optionsMonitor)
    {
        this.connectionManager = connectionManager;
        this.loggerFactory = loggerFactory;
        this.optionsMonitor = optionsMonitor;
        logger = loggerFactory.CreateLogger<ChannelManager>();
    }
    
    /// <inheritdoc />
    public ManagedChannel CreateChannel(string name)
    {
        logger.LogInformation("Creating channel manager for the RabbitMQ cluster {name}", name);

        var managedConnection = connectionManager.GetConnection(name);

        var managedChannel = new ExclusiveManagedChannel(
            managedConnection,
            loggerFactory.CreateLogger<ExclusiveManagedChannel>());

        return managedChannel;
    }

    /// <inheritdoc />
    public Task<ManagedChannel> GetPooledChannelAsync(string name, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ManagedChannel GetSharedChannel(string name) => sharedChannel ??= SafeCreateSharedChannel(name);

    private ManagedChannel SafeCreateSharedChannel(string name)
    {
        lock(logger)
        {
            if (sharedChannel is not null)
                return sharedChannel;
            
            logger.LogInformation("Creating shared channel manager for the RabbitMQ cluster {name}", name);
            
            var managedConnection = connectionManager.GetConnection(name);
            var managedChannel = new SharedManagedChannel(
                managedConnection,
                loggerFactory.CreateLogger<SharedManagedChannel>());

            return managedChannel;
        }
    }
}




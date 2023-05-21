using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RoyalCode.RabbitMQ.Components.Connections;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <inheritdoc />
internal sealed class ChannelManagerFactory : IChannelManagerFactory, IDisposable
{
    private readonly ConnectionManager connectionManager;
    private readonly ILoggerFactory loggerFactory;
    private readonly IOptionsMonitor<ChannelPoolOptions> optionsMonitor;
    private readonly Dictionary<string, ChannelManager> channelManagers = new();

    private readonly ILogger logger;

    private bool disposed;

    /// <summary>
    /// Creates a new channel manager factory.
    /// </summary>
    /// <param name="connectionManager"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="optionsMonitor"></param>
    public ChannelManagerFactory(
        ConnectionManager connectionManager,
        ILoggerFactory loggerFactory,
        IOptionsMonitor<ChannelPoolOptions> optionsMonitor)
    {
        this.connectionManager = connectionManager;
        this.loggerFactory = loggerFactory;
        this.optionsMonitor = optionsMonitor;

        logger = loggerFactory.CreateLogger<ChannelManagerFactory>();
    }

    /// <inheritdoc />
    public IChannelManager GetChannelManager(string name)
    {
        if (channelManagers.TryGetValue(name, out var channelManager))
            return channelManager;

        lock (channelManagers)
        {
            if (channelManagers.TryGetValue(name, out channelManager))
                return channelManager;

            logger.LogInformation("Creating channel manager for the RabbitMQ cluster {name}", name);

            channelManager = new ChannelManager(
                name,
                connectionManager.GetConnection(name),
                loggerFactory,
                optionsMonitor);

            channelManagers.Add(name, channelManager);
        }

        return channelManager;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;

        lock (channelManagers)
        {
            foreach (var channelManager in channelManagers.Values)
            {
                channelManager.Dispose();
            }
            channelManagers.Clear();
        }

        throw new NotImplementedException();
    }
}

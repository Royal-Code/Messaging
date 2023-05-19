
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RoyalCode.RabbitMQ.Components.Connections;
using System.Threading.Channels;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// <para>
///     Factory to create or get a channel manager for a RabbitMQ cluster.
/// </para>
/// </summary>
public interface IChannelManagerFactory
{
    /// <summary>
    /// <para>
    ///     Get or Creates a new channel manager for the RabbitMQ cluster.
    /// </para>
    /// </summary>
    /// <param name="name">The name of the RabbitMQ cluster.</param>
    /// <returns>The channel manager.</returns>
    IChannelManager GetChannelManager(string name);
}

internal sealed class ChannelManagerFactory : IChannelManagerFactory, IDisposable
{
    private readonly ConnectionManager connectionManager;
    private readonly ILoggerFactory loggerFactory;
    private readonly IOptionsMonitor<ChannelPoolOptions> optionsMonitor;
    private readonly Dictionary<string, ChannelManager> channelManagers = new();

    private readonly ILogger logger;

    private bool disposed;

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

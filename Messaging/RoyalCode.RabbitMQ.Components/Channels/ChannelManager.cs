using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Connections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <inheritdoc />
public sealed class ChannelManager : IChannelManager
{
    private readonly ConnectionManager connectionManager;
    private readonly ConcurrentDictionary<string, ConnectionConsumer> connectionConsumers = new();

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
        LoggerFactory = loggerFactory;
        OptionsMonitor = optionsMonitor;
    }
    
    private ILoggerFactory LoggerFactory { get; }
    
    private IOptionsMonitor<ChannelPoolOptions> OptionsMonitor { get; }
    
    /// <inheritdoc />
    public ManagedChannel CreateChannel(string name)
    {
        var consumer = GetConnectionConsumer(name);
        var options = OptionsMonitor.Get(name);

    }

    /// <inheritdoc />
    public Task<ManagedChannel> GetPooledChannelAsync(string name, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ManagedChannel GetSharedChannel(string name)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the connection consumer or create a new one for the given name of the RabbitMQ cluster.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ConnectionConsumer GetConnectionConsumer(string name)
    {
        return connectionConsumers.GetOrAdd(name, (name, conectionManager) =>
        {
            var managedConnection = conectionManager.GetConnection(name);

            return new ConnectionConsumer(managedConnection);
        }, connectionManager);
    }

    private class ConnectionConsumer : IConnectionConsumer
    {
        private readonly IConnectionConsumerStatus consumerStatus;
        private IConnection? connection;
        private bool closed;

        public ConnectionConsumer(ManagedConnection managedConnection)
        {
            consumerStatus = managedConnection.AddConsumer(this);
        }

        public void Closed()
        {
            connection = null;
            closed = true;
        }

        public void Consume(IConnection connection)
        {
            this.connection = connection;
        }

        public void Reloaded(IConnection connection, bool autorecovered)
        {
            this.connection = connection;
        }
    }
}




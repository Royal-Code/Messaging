using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RoyalCode.RabbitMQ.Components.Connections;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <inheritdoc />
public class ChannelManager : IChannelManager
{
    private readonly ConnectionManager connectionManager;
    private readonly Dictionary<string, ConnectionConsumer> connectionConsumers = new();

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
    public IChannelConsumerStatus Consume(string name, IChannelConsumer consumer)
    {
        if (connectionConsumers.TryGetValue(name, out var connectionConsumer))
            return connectionConsumer.AddConsumer(consumer);
        
        lock (connectionConsumers)
        {
            if (connectionConsumers.ContainsKey(name))
            {
                connectionConsumer = connectionConsumers[name];
            }
            else
            {
                connectionConsumer = new ConnectionConsumer(this, name, connectionManager);
                connectionConsumers.Add(name, connectionConsumer);
            }
        }
        
        return connectionConsumer.AddConsumer(consumer);
    }

    private void RemoveConnectionConsumer(string name)
    {
        lock (connectionConsumers)
        {
            if (connectionConsumers.ContainsKey(name))
                connectionConsumers.Remove(name);
        }
    }
}

internal class ConnectionConsumer : IConnectionConsumer
{
    private readonly object locker = new();
    private readonly ChannelManager channelManager;
    private readonly string name;
    private readonly LinkedList<ManagedConsumer> consumers = new();
    private DefaultChannelProvider? channelProvider;

    public ConnectionConsumer(ChannelManager channelManager, string name, ConnectionManager connectionManager)
    {
        this.channelManager = channelManager;
        this.name = name;
        connectionManager.Consume(name, this);
    }

    public void Dispose()
    {
        lock (locker)
        {
            channelManager.RemoveConnectionConsumer(name);
            channelProvider?.Dispose();
            consumers.Clear();
        }
    }

    public IChannelConsumerStatus AddConsumer(IChannelConsumer consumer)
    {
        var managed = new ManagedConsumer(this, consumer);

        lock (locker)
        {
            consumers.AddLast(managed);
            if (channelProvider is not null)
                managed.Consume(channelProvider);
        }

        return managed;
    }

    public void RemoveConsumer(ManagedConsumer managedConsumer)
    {
        lock (locker)
        {
            consumers.Remove(managedConsumer);
        }
    }

    public void Consume(IConnectionProvider connectionProvider)
    {
        lock (locker)
        {
            channelProvider = new DefaultChannelProvider(
                connectionProvider,
                channelManager.OptionsMonitor.Get(name),
                channelManager.LoggerFactory.CreateLogger<DefaultChannelProvider>());

            foreach (var consumer in consumers)
            {
                consumer.Consume(channelProvider);
            }
        }
    }

    public void Reload(bool autorecovered)
    {
        lock (locker)
        {
            foreach (var consumer in consumers)
            {
                consumer.Reload(autorecovered);
            }
        }
    }

    public void Closed()
    {
        lock (locker)
        {
            foreach (var consumer in consumers)
            {
                consumer.Closed();
            }
        }
    }
}

internal class ManagedConsumer : IChannelConsumerStatus
{
    private readonly ConnectionConsumer connectionConsumer;
    private readonly IChannelConsumer consumer;

    public ManagedConsumer(ConnectionConsumer connectionConsumer, IChannelConsumer consumer)
    {
        this.connectionConsumer = connectionConsumer;
        this.consumer = consumer;
    }

    public void Consume(IChannelProvider channelProvider) => consumer.Consume(channelProvider);

    public void Reload(bool autorecovered) => consumer.ConnectionRecovered(autorecovered);

    public void Closed() => consumer.ConnectionClosed();

    public void Dispose()
    {
        connectionConsumer.RemoveConsumer(this);
    }
}
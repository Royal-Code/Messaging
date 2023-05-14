using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace RoyalCode.RabbitMQ.Components.Connections;

/// <summary>
/// <para>
///     A component that manages a RabbitMQ connection.
/// </para>
/// </summary>
public sealed class ManagedConnection
{
    private readonly IConnectionPool connectionPool;
    private readonly ILogger logger;
    private readonly LinkedList<ManagedConsumer> consumers = new();
    private readonly EventHandler<ShutdownEventArgs> shutdownEventHandler;

    private IConnection? currentConnection;
    private bool error;

    /// <summary>
    /// Creates a new managed connection for a RabbitMQ cluster with a connection pool.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="connectionPool"></param>
    /// <param name="logger"></param>
    public ManagedConnection(string name, IConnectionPool connectionPool, ILogger logger)
    {
        Name = name;
        this.connectionPool = connectionPool;
        this.logger = logger;

        shutdownEventHandler = OnConnectionClosed;
    }

    /// <summary>
    /// The RabbitMQ Cluster name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// If is connected.
    /// </summary>
    public bool IsConnected => currentConnection?.IsOpen ?? false;

    /// <summary>
    /// Count of connection consumers.
    /// </summary>
    public int ConsumersCount
    {
        get
        {
            int count;
            lock (consumers)
            {
                count = consumers.Count;
            }
            return count;
        }
    }

    /// <summary>
    /// <para>
    ///     Get the current connection. If not connected, it will try to connect.
    /// </para>
    /// </summary>
    public IConnection? Connection
    {
        get
        {
            if (currentConnection is { IsOpen: true })
                return currentConnection;

            TryGetOrCreateConnection(out var conn);
            return conn;
        }
    }

    /// <summary>
    /// Adds a new consumer to the connection.
    /// </summary>
    /// <param name="consumer">The consumer.</param>
    /// <returns></returns>
    public IConnectionConsumerStatus AddConsumer(IConnectionConsumer consumer)
    {
        var managed = new ManagedConsumer(this, consumer);
        lock (consumers)
        {
            consumers.AddLast(managed);
        }

        ConsumeConnection(managed);
        return managed;
    }

    private void ConsumeConnection(ManagedConsumer consumer)
    {
        if (currentConnection is not null)
        {
            logger.LogDebug("Adding a consumer to current RabbitMQ connection for cluster name {Name}", Name);

            consumer.Consume(currentConnection, false);
            return;
        }

        if (TryGetOrCreateConnection(out var connection))
        {
            logger.LogDebug("Adding a consumer to a new RabbitMQ connection for cluster name {Name}", Name);

            consumer.Consume(connection!, false);
        }
    }

    private bool TryGetOrCreateConnection(out IConnection? connection)
    {
        if (error)
        {
            connection = null;
            return false;
        }

        lock (connectionPool)
        {
            if (currentConnection is not null)
            {
                connection = currentConnection;
                return true;
            }

            logger.LogInformation("Creating a RabbitMQ connection for cluster name {Name}", Name);

            try
            {
                currentConnection = connectionPool.GetNextConnetion();
                Connected();
                connection = currentConnection;

                logger.LogInformation("Connection to RabbitMQ realized for cluster name {Name}", Name);

                return true;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while creating a RabbitMQ connection for cluster name {Name}", Name);
                error = true;
                Reconnect();
                connection = null;
                return false;
            }
        }
    }

    private void Connected()
    {
        if (currentConnection is null)
            throw new InvalidOperationException();

        currentConnection.ConnectionShutdown += shutdownEventHandler;
    }

    private void Reconnect()
    {
        connectionPool.TryReconnect(Reconnected);
    }

    private void Reconnected(IConnection connection, bool autorecovered)
    {
        currentConnection = connection;
        error = false;
        Connected();

        lock (consumers)
        {
            foreach (var consumer in consumers)
            {
                consumer.Consume(connection, autorecovered);
            }
        }
    }

    private void OnConnectionClosed(object? sender, ShutdownEventArgs e)
    {
        error = true;

        logger.LogError(
            "An error occurred on a RabbitMQ connection for cluster name {Name}, a reconnection attempt will be made shortly, origin: {Initiator}, cause: {Cause}",
            Name, e.Initiator, e.Cause);

        if (currentConnection is not null)
        {
            currentConnection.ConnectionShutdown -= shutdownEventHandler;
            currentConnection = null;
        }

        Reconnect();
    }

    private void ReleaseConsumer(ManagedConsumer consumer)
    {
        lock (consumers)
        {
            consumers.Remove(consumer);
        }
    }

    private class ManagedConsumer : IConnectionConsumerStatus
    {
        private readonly ManagedConnection managed;
        private readonly IConnectionConsumer consumer;
        private IConnection? connection;
        private bool first = true;

        internal ManagedConsumer(ManagedConnection managed, IConnectionConsumer consumer)
        {
            this.managed = managed;
            this.consumer = consumer;
        }

        internal void Consume(IConnection connection, bool autorecovered)
        {
            this.connection = connection;
            if (first)
            {
                consumer.Consume(connection);
                first = false;
            }
            else
            {
                consumer.Reloaded(connection, autorecovered);
            }
        }

        /// <summary>
        /// Check if the consumer is connected to the RabbitMQ node.
        /// </summary>
        public bool IsConnected => connection?.IsOpen ?? false;

        /// <summary>
        /// Release the consumer.
        /// </summary>
        public void ReleaseConsumer()
        {
            managed.ReleaseConsumer(this);
            connection = null;
        }
    }
}


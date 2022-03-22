
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace RoyalCode.RabbitMQ.Components.Connections;

/// <summary>
/// <para>
///     Manage connection for RabbitMQ Clusters and register connection consumers for comsumers and publishers.
/// </para>
/// <para>
///     To configure the connections, use the <see cref="ConnectionPoolOptions"/>.
///     It can be configurated using the extension method 
///     <see cref="RabbitMqComponentsServiceCollectionExtensions.ConfigureRabbitMQConnection(IServiceCollection, string, Action{ConnectionPoolOptions})"/>
/// </para>
/// </summary>
public class ConnectionManager
{
    private readonly ConnectionPoolFactory connectionPoolFactory;
    private readonly ILoggerFactory loggerFactory;
    private readonly Dictionary<string, ManagedConnection> pools = new();

    /// <summary>
    /// Creates a new Connection Manager.
    /// </summary>
    /// <param name="connectionPoolFactory">The factory to create the connections pools.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public ConnectionManager(ConnectionPoolFactory connectionPoolFactory, ILoggerFactory loggerFactory)
    {
        this.connectionPoolFactory = connectionPoolFactory;
        this.loggerFactory = loggerFactory;
    }

    /// <summary>
    /// <para>
    ///     Consumes connections from a RabbitMQ Cluster for a given name.
    /// </para>
    /// <para>
    ///     If there is not already an open connection, it will be opened.
    ///     When the connection exists, the consumer will be invoked immediately.
    ///     If it is not possible to connect at the moment, 
    ///     a connection will be attempted later and when the connection is successful, 
    ///     the consumer will be invoked.
    /// </para>
    /// </summary>
    /// <param name="name">The name of the RabbitMQ Cluster.</param>
    /// <param name="consumer">The connection consumer.</param>
    /// <returns>True if was connected, false if has a connection error.</returns>
    public bool Consume(string name, IConnectionConsumer consumer)
    {
        if (pools.TryGetValue(name, out var managed)) 
            return managed.AddConsumer(consumer);
        
        lock (pools)
        {
            if (pools.ContainsKey(name))
            {
                managed = pools[name];
            }
            else
            {
                var pool = connectionPoolFactory.Create(name);
                var logger = loggerFactory.CreateLogger($"{GetType().FullName}.{nameof(ManagedConnection)}.{name}");
                managed = new ManagedConnection(name, pool, logger);
                pools[name] = managed;
            }
        }

        return managed.AddConsumer(consumer);
    }

    private class ManagedConnection
    {
        private readonly string name;
        private readonly IConnectionPool connectionPool;
        private readonly ILogger logger;
        private readonly LinkedList<ManagedConsumer> consumers = new();
        private readonly EventHandler<ShutdownEventArgs> shutdownEventHandler;

        private IConnection? currentConnection;
        private bool error;

        public ManagedConnection(string name, IConnectionPool connectionPool, ILogger logger)
        {
            this.name = name;
            this.connectionPool = connectionPool;
            this.logger = logger;

            shutdownEventHandler = OnConnectionClosed;
        }

        public bool AddConsumer(IConnectionConsumer consumer)
        {
            var managed = new ManagedConsumer(this, consumer);
            lock (consumers)
            {
                consumers.AddLast(managed);
            }

            return ConsumeConnection(managed);
        }

        private bool ConsumeConnection(ManagedConsumer consumer)
        {
            if (currentConnection is not null)
            {
                logger.LogDebug("Adding a consumer to current RabbitMQ connection for cluster name {Name}", name);

                consumer.Consume(currentConnection, false);
                return true;
            }

            if (TryGetOrCreateConnection(out var connection))
            {
                logger.LogDebug("Adding a consumer to a new RabbitMQ connection for cluster name {Name}", name);

                consumer.Consume(connection!, false);

                return true;
            }

            return false;
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

                logger.LogInformation("Creating a RabbitMQ connection for cluster name {Name}", name);

                try
                {
                    currentConnection = connectionPool.GetNextConnetion();
                    Connected();
                    connection = currentConnection;

                    logger.LogInformation("Connection to RabbitMQ realized for cluster name {Name}", name);

                    return true;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error while creating a RabbitMQ connection for cluster name {Name}", name);
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
            logger.LogError(
                "An error occurred on a RabbitMQ connection for cluster name {Name}, a reconnection attempt will be made shortly, origin: {Initiator}, cause: {Cause}",
                name, e.Initiator, e.Cause);

            error = true;

            if (currentConnection is not null)
            {
                currentConnection.ConnectionShutdown -= shutdownEventHandler;
                currentConnection = null;
            }

            Reconnect();
        }

        internal void ReleaseConsumer(ManagedConsumer consumer)
        {
            lock (consumers)
            {
                consumers.Remove(consumer);
            }
        }
    }

    private class ManagedConsumer
    {
        private readonly ManagedConnection managed;
        private readonly IConnectionConsumer consumer;
        private ConnectionProvider? connectionProvider;
        private bool first = true;

        public ManagedConsumer(ManagedConnection managed, IConnectionConsumer consumer)
        {
            this.managed = managed;
            this.consumer = consumer;
        }

        public void Consume(IConnection connection, bool autorecovered)
        {
            if (first)
            {
                connectionProvider = new ConnectionProvider(connection, ReleaseConsumer);

                consumer.Consume(connectionProvider);
                first = false;
            }
            else
            {
                connectionProvider!.Connection = connection;

                consumer.Reload(autorecovered);
            }
        }

        private void ReleaseConsumer()
        {
            managed.ReleaseConsumer(this);
        }
    }

    private sealed class ConnectionProvider : IConnectionProvider
    {
        private readonly Action disposeCallback;

        public ConnectionProvider(IConnection connection, Action disposeCallback)
        {
            Connection = connection;
            this.disposeCallback = disposeCallback;
        }

        public IConnection Connection { get; internal set; }

        public bool IsOpen => Connection.IsOpen;

        public void Dispose() => disposeCallback();
    }
}


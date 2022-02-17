
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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
    /// </summary>
    /// <param name="name">The name of the RabbitMQ Cluster.</param>
    /// <param name="consumer">The connection consumer.</param>
    public void Consume(string name, IConnectionConsumer consumer)
    {
        if (pools.ContainsKey(name))
        {
            pools[name].AddConsumer(consumer);
            return;
        }

        ManagedConnection managed;
        lock (pools)
        {
            if (pools.ContainsKey(name))
            { 
                managed = pools[name];
            }
            else
            { 
                var pool = connectionPoolFactory.Create(name);
                var logger = loggerFactory.CreateLogger($"{nameof(ManagedConnection)}_{name}");
                managed = new ManagedConnection(name, pool, logger);
                pools[name] = managed;
            }
        }

        managed.AddConsumer(consumer);
    }

    private class ManagedConnection
    {
        private readonly string name;
        private readonly IConnectionPool connectionPool;
        private readonly ILogger logger;
        private readonly LinkedList<IConnectionConsumer> consumers = new();
        private readonly EventHandler<ShutdownEventArgs> shutdownEventHandler;

        private IConnection? currentConnection;
        private bool error = false;

        public ManagedConnection(string name, IConnectionPool connectionPool, ILogger logger)
        {
            this.name = name;
            this.connectionPool = connectionPool;
            this.logger = logger;

            shutdownEventHandler = OnConnectionClosed;
        }

        public void AddConsumer(IConnectionConsumer consumer)
        {
            lock (consumers)
            {
                consumers.AddLast(consumer);
            }

            ConsumeConnection(consumer);
        }

        private void ConsumeConnection(IConnectionConsumer consumer)
        {
            if (currentConnection is not null)
            {
                logger.LogDebug("Adding a consumer to current RabbitMQ connection for cluster name {0}", name);

                consumer.Consume(currentConnection);
                return;
            }

            if (TryCreateConnection(out var connection))
            {
                logger.LogDebug("Adding a consumer to a new RabbitMQ connection for cluster name {0}", name);

                consumer.Consume(connection!);
            }
        }

        public bool TryCreateConnection(out IConnection? connection)
        {
            if (error)
            {
                connection = null;
                return false;
            }

            lock(connectionPool)
            {
                if (currentConnection is not null)
                {
                    connection = currentConnection;
                    return true;
                }

                logger.LogInformation("Creating a RabbitMQ connection for cluster name {0}", name);

                try
                {
                    currentConnection = connectionPool.GetNextConnetion();
                    Connected();
                    connection = currentConnection;

                    logger.LogInformation("Connection to RabbitMQ realized for cluster name {0}", name);

                    return true;
                }
                catch(Exception e)
                {
                    logger.LogError(e, "Error while creating a RabbitMQ connection for cluster name {0}", name);
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

        private void Reconnected(IConnection connection)
        {
            currentConnection = connection;
            Connected();


        }

        private void OnConnectionClosed(object sender, ShutdownEventArgs e)
        {
            logger.LogError( 
                "An error occurred on a RabbitMQ connection for cluster name {0}, a reconnection attempt will be made shortly, origin: {1}, cause: {2}", 
                name, e.Initiator, e.Cause);

            error = true;
            currentConnection = null;
            Reconnect();
        }


    }
}


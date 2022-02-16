
using Microsoft.Extensions.DependencyInjection;
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
    private readonly Dictionary<string, ManagedConnection> pools = new();

    /// <summary>
    /// Creates a new Connection Manager.
    /// </summary>
    /// <param name="connectionPoolFactory">The factory to create the connections pools.</param>
    public ConnectionManager(ConnectionPoolFactory connectionPoolFactory)
    {
        this.connectionPoolFactory = connectionPoolFactory;
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
                managed = new ManagedConnection(pool);
                pools[name] = managed;
            }
        }

        managed.AddConsumer(consumer);
    }

    private class ManagedConnection
    {
        private readonly IConnectionPool connectionPool;
        private IConnection? currentConnection;
        private readonly LinkedList<IConnectionConsumer> consumers = new();

        public ManagedConnection(IConnectionPool connectionPool)
        {
            this.connectionPool = connectionPool;
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
                consumer.Consume(currentConnection);
                return;
            }


        }

        public bool TryCreateConnection(out IConnection connection)
        {
            lock(connectionPool)
            {
                if (currentConnection is not null)
                {
                    connection = currentConnection;
                    return true;
                }

                try
                {
                    currentConnection = connectionPool.GetNextConnetion();

                }
                catch(Exception e)
                {

                }
            }
        }
    }
}


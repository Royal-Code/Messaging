
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
public sealed class ConnectionManager
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
    ///     Get a managed connection for a RabbitMQ Cluster.
    /// </para>
    /// <para>
    ///     If a managed connection already exists for the cluster, it will be returned.
    /// </para>
    /// </summary>
    /// <param name="name">The name of the RabbitMQ Cluster.</param>
    /// <returns>An instance of <see cref="ManagedConnection"/>.</returns>
    public ManagedConnection GetConnection(string name)
    {
        if (pools.TryGetValue(name, out var managed))
            return managed;

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

        return managed;
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
    public IConnectionConsumerStatus Consume(string name, IConnectionConsumer consumer)
    {
        return GetConnection(name).AddConsumer(consumer);
    }
}
using RabbitMQ.Client;

namespace RoyalCode.RabbitMQ.Components.Connections;

/// <summary>
/// <para>
///     A pool of connection of an RabbitMQ Cluster.
/// </para>
/// <para>
///     It is used by the <see cref="ConnectionManager"/> to creates connection with RabbitMQ nodes.
/// </para>
/// </summary>
public interface IConnectionPool
{
    /// <summary>
    /// Get the next connection from a RabbitMQ node.
    /// </summary>
    /// <returns>Main interface for a AMQP connection, with RabbitMQ.</returns>
    IConnection GetNextConnetion();
}

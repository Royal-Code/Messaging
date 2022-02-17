using RabbitMQ.Client;
using System;

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

    /// <summary>
    /// <para>
    ///     It runs a routine that tries to connect to some node in the cluster. When connected, 
    ///     the callback action is executed.
    /// </para>
    /// <para>
    ///     If the cluster is configured to return to the primary node, 
    ///     the callback action is executed again when the connection to the primary node is established
    /// </para>
    /// </summary>
    /// <param name="callback">the callback action to receive the next connection.</param>
    void TryReconnect(Action<IConnection> callback);
}

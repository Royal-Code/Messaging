using RabbitMQ.Client;
using System;

namespace RoyalCode.RabbitMQ.Components.Connections;

/// <summary>
/// <para>
///     Component that encapsulates the connection to RabbitMQ.
/// </para>
/// <para>
///     When the connection is no longer needed, dispose must be called on this object.
///     This will not close the connection, 
///     but will inform the connection manager that the consumer no longer uses the connection.
/// </para>
/// </summary>
public interface IConnectionProvider : IDisposable
{
    /// <summary>
    /// A connection to RabbitMQ.
    /// </summary>
    IConnection Connection { get; }

    /// <summary>
    /// If the connection is open.
    /// </summary>
    bool IsOpen { get; }
}

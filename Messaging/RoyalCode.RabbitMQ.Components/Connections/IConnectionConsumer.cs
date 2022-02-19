using System;

namespace RoyalCode.RabbitMQ.Components.Connections;

/// <summary>
/// <para>
///     Component to consume connections to RabbitMQ via <see cref="ConnectionManager"/>.
/// </para>
/// <para>
///     The <see cref="ConnectionManager"/> will establish a connection to RabbitMQ 
///     and provide it to the consumer.
/// </para>
/// <para>
///     When disconnections occur, the <see cref="ConnectionManager"/> will reconnect to RabbitMQ 
///     and notify the connection consumers.
/// </para>
/// </summary>
public interface IConnectionConsumer : IDisposable
{
    /// <summary>
    /// <para>
    ///     This method will be triggered by <see cref="ConnectionManager"/> as soon as you add it as a consumer
    ///     and there is an open connection.
    /// </para>
    /// <para>
    ///     The connection will be opened as soon as the first consumer is added to 
    ///     the <see cref="ConnectionManager"/>. Subsequent consumers will receive the previously 
    ///     opened connection immediately.
    /// </para>
    /// </summary>
    /// <param name="connection">An open connection with RabbitMQ.</param>
    void Consume(IConnectionProvider connection);

    /// <summary>
    /// <para>
    ///     When a disconnection occurs, <see cref="ConnectionManager"/> will attempt to establish
    ///     a new connection, and this method will be triggered for all consumers.
    /// </para>
    /// <para>
    ///     When the new connection is received by consumers, the channels and message receivers must be recreated.
    /// </para>
    /// </summary>
    /// <param name="autorecovered">If the connection was auto recovered</param>
    void Reload(bool autorecovered);

    /// <summary>
    /// Informe all consumers that the connection was closed.
    /// </summary>
    void Closed();
}
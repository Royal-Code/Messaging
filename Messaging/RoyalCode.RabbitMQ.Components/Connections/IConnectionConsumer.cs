using System;

namespace RoyalCode.RabbitMQ.Components.Connections;

/// <summary>
/// <para>
///     Component to consume connections to RabbitMQ via <see cref="ConnectionManager"/>.
/// </para>
/// <para>
///     The <see cref="ConnectionManager"/> will establish a connectionProvider to RabbitMQ 
///     and provide it to the consumer.
/// </para>
/// <para>
///     When disconnections occur, the <see cref="ConnectionManager"/> will reconnect to RabbitMQ 
///     and notify the connectionProvider consumers.
/// </para>
/// </summary>
public interface IConnectionConsumer : IDisposable
{
    /// <summary>
    /// <para>
    ///     This method will be triggered by <see cref="ConnectionManager"/> as soon as you add it as a consumer
    ///     and there is an open connectionProvider.
    /// </para>
    /// <para>
    ///     The connectionProvider will be opened as soon as the first consumer is added to 
    ///     the <see cref="ConnectionManager"/>. Subsequent consumers will receive the previously 
    ///     opened connectionProvider immediately.
    /// </para>
    /// </summary>
    /// <param name="connectionProvider">An open connectionProvider with RabbitMQ.</param>
    void Consume(IConnectionProvider connectionProvider);

    /// <summary>
    /// <para>
    ///     When a disconnection occurs, <see cref="ConnectionManager"/> will attempt to establish
    ///     a new connectionProvider, and this method will be triggered for all consumers.
    /// </para>
    /// <para>
    ///     When the new connectionProvider is received by consumers, the channels and message receivers must be recreated.
    /// </para>
    /// </summary>
    /// <param name="autorecovered">If the connectionProvider was auto recovered</param>
    void Reload(bool autorecovered);

    /// <summary>
    /// Informe all consumers that the connectionProvider was closed.
    /// </summary>
    void Closed();
}
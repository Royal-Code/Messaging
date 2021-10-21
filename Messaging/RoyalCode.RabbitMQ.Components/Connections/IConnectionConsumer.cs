using RabbitMQ.Client;
using System;

namespace RoyalCode.RabbitMQ.Components.Connections
{
    /// <summary>
    /// <para>
    ///     Component to consume connections to RabbitMQ via <see cref="ConnectionManagement"/>.
    /// </para>
    /// <para>
    ///     The <see cref="ConnectionManagement"/> will establish a connection to RabbitMQ 
    ///     and provide it to the consumer.
    /// </para>
    /// <para>
    ///     When disconnections occur, the <see cref="ConnectionManagement"/> will reconnect to RabbitMQ 
    ///     and notify the connection consumers.
    /// </para>
    /// </summary>
    public interface IConnectionConsumer : IDisposable
    {
        /// <summary>
        /// <para>
        ///     This method will be triggered by <see cref="ConnectionManagement"/> as soon as you add it as a consumer
        ///     and there is an open connection.
        /// </para>
        /// <para>
        ///     The connection will be opened as soon as the first consumer is added to 
        ///     the <see cref="ConnectionManagement"/>. Subsequent consumers will receive the previously 
        ///     opened connection immediately.
        /// </para>
        /// </summary>
        /// <param name="connection">An open connection with RabbitMQ.</param>
        void Consume(IConnection connection);

        /// <summary>
        /// <para>
        ///     When a disconnection occurs, <see cref="ConnectionManagement"/> will attempt to establish
        ///     a new connection, and this method will be triggered for all consumers.
        /// </para>
        /// <para>
        ///     When the new connection is received by consumers, the channels and message receivers must be recreated.
        /// </para>
        /// </summary>
        /// <param name="connection"></param>
        void Reload(IConnection connection);
    }
}

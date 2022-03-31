namespace RoyalCode.Messaging;

/// <summary>
/// A messaging component for publishing messages to one or more brokers 
/// determined by the configuration of the message type (class).
/// </summary>
/// <typeparam name="TMessage">Modelo de dado da mensagem.</typeparam>
public interface IPublisher<TMessage> : IDisposable
{
    /// <summary>
    /// Sends a message with the data model information.
    /// </summary>
    /// <param name="instance">Data model to send in the message.</param>
    /// <param name="token">Cancellation token.</param>
    Task PublishAsync(TMessage instance, CancellationToken token = default);

    /// <summary>
    /// Sends a message with the data model information for a given route.
    /// </summary>
    /// <param name="instance">Data model to send in the message.</param>
    /// <param name="routeKey">The route key.</param>
    /// <param name="token">Cancellation token.</param>
    Task PublishAsync(TMessage instance, string routeKey, CancellationToken token = default);
}

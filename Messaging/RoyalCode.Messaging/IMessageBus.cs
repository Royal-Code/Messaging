namespace RoyalCode.Messaging;

/// <summary>
/// <para>
///     Component for sending and receiving messages between systems via a messaging broker.
/// </para>
/// <para>
///     This is the main component of these libraries for publishing and receiving messages.
/// </para>
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// Gets a message publisher of a certain type.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <returns>An instance of a message publisher.</returns>
    IPublisher<TMessage> CreatePublisher<TMessage>();

    /// <summary>
    /// Gets a service for listening to message queues of a certain type.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <returns>An instance of a message receiver.</returns>
    IReceiver<TMessage> CreateReceiver<TMessage>();

    /// <summary>
    /// Sends a message with the data model information.
    /// </summary>
    /// <param name="instance">The message to send.</param>
    Task Publish<TMessage>(TMessage instance);
}

namespace RoyalCode.Messaging.Abstractions.Bus;

/// <summary>
/// <para>
///     Component to create <see cref="IPublisher{TMessage}"/> configurated to use.
/// </para>
/// <para>
///     Each broker library must implement this provider to create the publishers.
/// </para>
/// </summary>
public interface IPublisherProvider
{
    /// <summary>
    /// <para>
    ///     Finds or creates a publisher for the message type
    ///     if the message is configured for publishing in the broker.
    /// </para>
    /// </summary>
    /// <typeparam name="TMessage">The message type</typeparam>
    /// <returns>A publisher or null if the message type is not configured for the broker.</returns>
    IPublisher<TMessage>? FindPublisher<TMessage>();
}
namespace RoyalCode.Messaging.Abstractions.Providers;

/// <summary>
/// <para>
///     Component to create <see cref="IReceiver{TMessage}"/> configurated to use.
/// </para>
/// <para>
///     Each broker library must implement this provider to create the receivers.
/// </para>
/// </summary>
public interface IReceiverProvider
{
    /// <summary>
    /// <para>
    ///     Finds or creates a receiver for the message type
    ///     if the message is configured to be listened to by the broker.
    /// </para>
    /// </summary>
    /// <typeparam name="TMessage">The message type</typeparam>
    /// <returns>A receiver or null if the message type is not configured for the broker.</returns>
    IReceiver<TMessage>? FindReceiver<TMessage>();
}
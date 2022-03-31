namespace RoyalCode.Messaging;

/// <summary>
/// Service/Component that listens to message queues and passes the message on to listeners.
/// </summary>
/// <typeparam name="TMessage">Object type received from messaging.</typeparam>
public interface IReceiver<TMessage> : IDisposable
{
    /// <summary>
    /// Starts listening to a channel by sending messages to the listener.
    /// </summary>
    /// <param name="handler">Component that processes the messages received from the channel.</param>
    void Listen(Func<TMessage, Task> handler);

    /// <summary>
    /// Starts listening to a channel by sending messages to the listener.
    /// </summary>
    /// <param name="handler">Component that processes the messages received from the channel.</param>
    void Listen(Func<IIncomingMessage<TMessage>, Task> handler);
}

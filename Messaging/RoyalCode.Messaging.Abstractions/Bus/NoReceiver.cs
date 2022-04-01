namespace RoyalCode.Messaging.Abstractions.Bus;

/// <summary>
/// Internal class that do nothing when a listener is added.
/// </summary>
/// <typeparam name="TMessage">The message type.</typeparam>
public class NoReceiver<TMessage> : IReceiver<TMessage>
{
    /// <inheritdoc />
    public void Dispose() { }

    /// <inheritdoc />
    public void Listen(Func<TMessage, Task> handler) { }
    
    /// <inheritdoc />
    public void Listen(Func<IIncomingMessage<TMessage>, Task> handler) { }
}

namespace RoyalCode.Messaging.Abstractions.Bus;

/// <summary>
/// Internal class to listen messages from multiples brokers.
/// </summary>
/// <typeparam name="TMessage">The message type.</typeparam>
public class MultiReceiver<TMessage> : IReceiver<TMessage>
{
    private readonly LinkedList<IReceiver<TMessage>> receivers = new();

    internal void AddReceiver(IReceiver<TMessage> receiver) => receivers.AddLast(receiver);
    
    /// <inheritdoc />
    public void Listen(Func<TMessage, Task> handler)
    {
        foreach (var receiver in receivers)
        {
            receiver.Listen(handler);
        }
    }

    /// <inheritdoc />
    public void Listen(Func<IIncomingMessage<TMessage>, Task> handler)
    {
        foreach (var receiver in receivers)
        {
            receiver.Listen(handler);
        }
    }
    
    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var receiver in receivers)
        {
            receiver.Dispose();
        }

        receivers.Clear();
    }
}
namespace RoyalCode.Messaging.Abstractions.Bus;

internal sealed class MultiPublisher<TMessage> : IPublisher<TMessage>
{
    private readonly LinkedList<IPublisher<TMessage>> publishers = new();

    internal void AddPublisher(IPublisher<TMessage> publisher) => publishers.AddLast(publisher);
    
    public Task PublishAsync(TMessage instance, CancellationToken token = default)
    {
        
        
        throw new NotImplementedException();
    }

    public Task PublishAsync(TMessage instance, string routeKey, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
    
    public void Dispose()
    {
        foreach (var publisher in publishers)
        {
            publisher.Dispose();
        }
        publishers.Clear();
    }
}
namespace RoyalCode.Messaging.Abstractions.Bus;

/// <summary>
/// Internal class that do nothing when a message is required to be published.
/// </summary>
/// <typeparam name="TMessage">The message type.</typeparam>
internal sealed class NoPublisher<TMessage> : IPublisher<TMessage>
{
    /// <inheritdoc />
    public void Dispose() { }

    /// <inheritdoc />
    public Task PublishAsync(TMessage instance, CancellationToken token = default) 
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task PublishAsync(TMessage instance, string routeKey, CancellationToken token = default) 
        => Task.CompletedTask;
}
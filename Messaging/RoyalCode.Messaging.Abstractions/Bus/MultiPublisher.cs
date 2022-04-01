
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using RoyalCode.Messaging.Abstractions.Handlers;

namespace RoyalCode.Messaging.Abstractions.Bus;

/// <summary>
/// Internal class to publish a message to multiples brokers.
/// </summary>
/// <typeparam name="TMessage">The message type.</typeparam>
internal sealed class MultiPublisher<TMessage> : IPublisher<TMessage>
{
    private readonly IExceptionHandlersFactory factory;
    private readonly ILogger logger;
    private readonly LinkedList<IPublisher<TMessage>> publishers = new();

    public MultiPublisher(IExceptionHandlersFactory factory, ILogger logger)
    {
        this.factory = factory;
        this.logger = logger;
    }

    internal void AddPublisher(IPublisher<TMessage> publisher) => publishers.AddLast(publisher);

    /// <inheritdoc />
    public Task PublishAsync(TMessage instance, CancellationToken token = default)
        => InternalPublishAsync(instance, null, token);

    /// <inheritdoc />
    public Task PublishAsync(TMessage instance, string routeKey, CancellationToken token = default)
        => InternalPublishAsync(instance, routeKey, token);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async Task InternalPublishAsync(TMessage instance, string? routeKey, CancellationToken token = default)
    {
        var result = new PublishResult(typeof(TMessage));
        
        foreach (var publisher in publishers)
        {
            var publisherType = publisher.GetType();
            
            try
            {
                if (routeKey is null)
                    await publisher.PublishAsync(instance, token);
                else
                    await publisher.PublishAsync(instance, routeKey, token);
                
                result.AddSuccessPublication(publisherType);
            }
            catch (Exception e)
            {
                logger.LogInformation(e, 
                    "An error occurred when publishing a message of type '{MessageTypeName}' via the publisher '{PublisherTypeName}'", 
                    typeof(TMessage).Name, 
                    publisherType.Name);
                
                var oex = result.AddPublicationException(publisherType, e);
                foreach (var handler in factory.GetPublishExceptionHandler(publisherType))
                {
                    handler.ExceptionOccured(instance, oex);
                }
            }
        }

        if (result.HasException)
            throw result.CreateException();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var publisher in publishers)
        {
            publisher.Dispose();
        }

        publishers.Clear();
    }
}
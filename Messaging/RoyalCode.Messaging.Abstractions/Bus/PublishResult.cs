using RoyalCode.Messaging.Abstractions.Exceptions;

namespace RoyalCode.Messaging.Abstractions.Bus;

/// <summary>
/// <para>
///     A class to keep the result of the publications when there are multiple publishers for a message type.
/// </para>
/// </summary>
public class PublishResult
{
    internal LinkedList<Type>? successPublished;
    internal LinkedList<OnPublishException>? exceptions;

    /// <summary>
    /// Creates a new result for multi publishers.
    /// </summary>
    /// <param name="messageType"></param>
    public PublishResult(Type messageType)
    {
        MessageType = messageType;
    }

    /// <summary>
    /// The published message type.
    /// </summary>
    public Type MessageType { get; }
    
    /// <summary>
    /// The number of publishers.
    /// </summary>
    public int PublishersCount { get; private set; }

    /// <summary>
    /// Check if there are any exceptions.
    /// </summary>
    public bool HasException => exceptions is not null;

    /// <summary>
    /// Adds the publisher type where the message was successfully published.
    /// </summary>
    /// <param name="publisherType">The publisher type.</param>
    public void AddSuccessPublication(Type publisherType)
    {
        PublishersCount++;
        successPublished ??= new();
        successPublished.AddLast(publisherType);
    }

    /// <summary>
    /// Adds the exception occurred when publishing a message.
    /// </summary>
    /// <param name="publisherType">The publisher type.</param>
    /// <param name="ex">The exception occurred</param>
    public OnPublishException AddPublicationException(Type publisherType, Exception ex)
    {
        if (ex is not OnPublishException oex)
            oex = new OnPublishException(MessageType, publisherType, ex);

        PublishersCount++;
        exceptions ??= new();
        exceptions.AddLast(oex);
        
        return oex;
    }

    /// <summary>
    /// Creates an exception for this result.
    /// </summary>
    /// <returns>A new <see cref="PublishResultException"/>.</returns>
    internal Exception CreateException() => new PublishResultException(this);
}
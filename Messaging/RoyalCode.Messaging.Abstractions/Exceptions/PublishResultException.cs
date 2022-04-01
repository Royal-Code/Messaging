using RoyalCode.Messaging.Abstractions.Bus;

namespace RoyalCode.Messaging.Abstractions.Exceptions;

/// <summary>
/// <para>
///     Exception of publishing a message to multiple brokers and some of these publications fail.
/// </para>
/// </summary>
public class PublishResultException : AggregateException
{
    /// <summary>
    /// Creates a new exception.
    /// </summary>
    /// <param name="result">The result.</param>
    public PublishResultException(PublishResult result)
        : base(string.Format(Resources.PublishResultException, result.MessageType), result.exceptions!)
    {
        MessageType = result.MessageType;
        PublishersCount = result.PublishersCount;
        SuccessfulPublishers = result.successPublished ?? Enumerable.Empty<Type>();
    }
    
    /// <summary>
    /// The published message type.
    /// </summary>
    public Type MessageType { get; }
    
    /// <summary>
    /// The number of publishers.
    /// </summary>
    public int PublishersCount { get; }
    
    /// <summary>
    /// The successful publishers.
    /// </summary>
    public IEnumerable<Type> SuccessfulPublishers { get; }
}
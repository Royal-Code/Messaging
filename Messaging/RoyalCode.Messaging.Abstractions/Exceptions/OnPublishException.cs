namespace RoyalCode.Messaging.Abstractions.Exceptions;

/// <summary>
/// An exception to encapsulate exceptions occurring during message publishing.
/// </summary>
public class OnPublishException : Exception
{
    /// <summary>
    /// Creates a new exception.
    /// </summary>
    /// <param name="messageType">See <see cref="MessageType"/>.</param>
    /// <param name="publisherType">See <see cref="PublisherType"/>.</param>
    /// <param name="cause">Cause exception.</param>
    public OnPublishException(Type messageType, Type publisherType, Exception cause)
        : base(string.Format(Resources.OnPublishException, messageType.Name, publisherType.Name), cause)
    {
        MessageType = messageType;
        PublisherType = publisherType;
    }
    
    /// <summary>
    /// The message type.
    /// </summary>
    public Type MessageType { get; }
    
    /// <summary>
    /// The publisher type.
    /// </summary>
    public Type PublisherType { get; }
}
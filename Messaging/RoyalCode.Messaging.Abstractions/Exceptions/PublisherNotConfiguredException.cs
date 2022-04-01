namespace RoyalCode.Messaging.Abstractions.Exceptions;

/// <summary>
/// <para>
///     Exception for when there is no configuration for publishing a message. 
/// </para>
/// </summary>
public class PublisherNotConfiguredException : Exception
{
    /// <summary>
    /// Creates a new exception for publisher not configured for <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Not configured message type.</param>
    public PublisherNotConfiguredException(Type type)
        : base(string.Format(Resources.PublishNotConfigured, type.Name))
    { }
}
namespace RoyalCode.Messaging.Abstractions.Options;

/// <summary>
/// <para>
///     Options for message bus components.
/// </para>
/// </summary>
public class MessagingOptions
{
    /// <summary>
    /// <para>
    ///     Whether to throw an exception if no publisher or receiver is configured for a message type,
    ///     when publishing or listening to messages.
    /// </para>
    /// <para>
    ///     The default value is true.
    /// </para>
    /// </summary>
    public bool ThrowIfNotFoundPublisherOrReceiver { get; set; } = true;
}
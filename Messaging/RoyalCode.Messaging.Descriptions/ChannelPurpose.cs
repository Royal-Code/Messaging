namespace RoyalCode.Messaging.Descriptions;

/// <summary>
/// <para>
///     Determines the purpose for which the message will be used by the application.
/// </para>
/// </summary>
public enum ChannelPurpose
{
    /// <summary>
    /// Used to publish messages.
    /// </summary>
    Publish,

    /// <summary>
    /// Used to receive messages.
    /// </summary>
    Receive,
}

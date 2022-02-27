namespace RoyalCode.RabbitMQ.Components.Communication;

/// <summary>
/// <para>
///     Used by <see cref="ChannelInfo"/> to define the way to publish or receive messages from RabbitMQ.
/// </para>
/// </summary>
public enum ChannelType
{
    /// <summary>
    /// Direct to a queue.
    /// </summary>
    Queue,

    /// <summary>
    /// Publish to an exchange or receive messages from a queue bound to an exchage.
    /// </summary>
    Exchange
}

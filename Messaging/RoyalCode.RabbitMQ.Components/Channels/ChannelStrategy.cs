namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// <para>
///     Strategies to consume the channels.
/// </para>
/// </summary>
public enum ChannelStrategy
{
    /// <summary>
    /// Exclusive channel per consumers, for each consumer will be a channel.
    /// </summary>
    Exclusive,

    /// <summary>
    /// A common and shared channel between all the consumers.
    /// </summary>
    Singleton,
    
    /// <summary>
    /// A channel from a pool.
    /// </summary>
    Pooled,
}
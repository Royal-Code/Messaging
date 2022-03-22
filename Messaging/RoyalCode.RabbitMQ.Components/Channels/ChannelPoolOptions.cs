namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// <para>
///     Options for pooled channels.
/// </para>
/// </summary>
public class ChannelPoolOptions
{
    /// <summary>
    /// The max size of the pool of channels.
    /// </summary>
    public int PoolMaxSize { get; set; } = 10;
}
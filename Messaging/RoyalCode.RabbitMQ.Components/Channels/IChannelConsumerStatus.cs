namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// <para>
///     A object that holds the status of a channel consumer, and allows the consumer to be released.
/// </para>
/// </summary>
public interface IChannelConsumerStatus : IDisposable
{
    /// <summary>
    /// Check if the channel consumer is open.
    /// </summary>
    public bool IsOpen { get; }
}

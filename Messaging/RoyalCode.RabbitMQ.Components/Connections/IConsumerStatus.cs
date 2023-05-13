
namespace RoyalCode.RabbitMQ.Components.Connections;

/// <summary>
/// <para>
///     Status of a consumer, returned byt the <see cref="ManagedConnection"/> when a consumer is added.
/// </para>
/// <para>
///     It can be used to check if the consumer is connected to the RabbitMQ node and
///     release the consumer.
/// </para>
/// </summary>
public interface IConsumerStatus
{
    /// <summary>
    /// Check if the consumer is connected to the RabbitMQ node.
    /// </summary>
    public bool IsConnected { get; }

    /// <summary>
    /// Release the consumer.
    /// </summary>
    public void ReleaseConsumer();
}
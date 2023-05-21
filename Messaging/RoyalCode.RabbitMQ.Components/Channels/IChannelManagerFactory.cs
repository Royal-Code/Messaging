
namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// <para>
///     Factory to create or get a channel manager for a RabbitMQ cluster.
/// </para>
/// </summary>
public interface IChannelManagerFactory
{
    /// <summary>
    /// <para>
    ///     Get or Creates a new channel manager for the RabbitMQ cluster.
    /// </para>
    /// </summary>
    /// <param name="name">The name of the RabbitMQ cluster.</param>
    /// <returns>The channel manager.</returns>
    IChannelManager GetChannelManager(string name);
}
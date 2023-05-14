using RabbitMQ.Client;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// <para>
///     RabbitMQ Channel Manager.
/// </para>
/// <para>
///     It allows channel (<see cref="IModel"/>) consumption and manages connections and reconnections.
/// </para>
/// </summary>
public interface IChannelManager
{
    /// <summary>
    /// Creates a new RabbitMQ managed channel, an object of type: <see cref="ManagedChannel"/>.
    /// </summary>
    /// <param name="name">The RabbitMQ cluster name.</param>
    /// <returns>A new <see cref="ManagedChannel"/>.</returns>
    ManagedChannel CreateChannel(string name);

    /// <summary>
    /// Get the RabbitMQ managed channel, an object of type: <see cref="ManagedChannel"/>, shared between all components.
    /// </summary>
    /// <param name="name">The RabbitMQ cluster name.</param>
    /// <returns>The shared <see cref="ManagedChannel"/>.</returns>
    ManagedChannel GetSharedChannel(string name);

    /// <summary>
    /// <para>
    ///     Get the RabbitMQ managed channel, an object of type: <see cref="IModel"/>, from a pool.
    /// </para>
    /// <para>
    ///     Once completed the publication, the method <see cref="ManagedChannel.Release()"/> is required
    ///     to be called for others components can use the channel.
    /// </para>
    /// </summary>
    /// <param name="name">The RabbitMQ cluster name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Task for async processing with the <see cref="IModel"/>.</returns>
    Task<ManagedChannel> GetPooledChannelAsync(string name, CancellationToken cancellationToken = default);
}

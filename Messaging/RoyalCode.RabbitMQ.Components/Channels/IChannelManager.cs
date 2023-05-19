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
    /// The name of the RabbitMQ cluster.
    /// </summary>
    public string ClusterName { get; }

    /// <summary>
    /// <para>
    ///     Creates a new RabbitMQ managed channel, an object of type: <see cref="ManagedChannel"/>.
    /// </para>
    /// <para>
    ///     This channel is recommended to use for both, publishers and consumers.
    /// </para>
    /// <para>
    ///     Once completed the publication, the method <see cref="IDisposable.Dispose()"/> is required
    ///     to close the channel.
    /// </para>
    /// </summary>
    /// <returns>A new <see cref="ManagedChannel"/>.</returns>
    ManagedChannel CreateChannel();

    /// <summary>
    /// <para>
    ///     Get the RabbitMQ managed channel, an object of type: <see cref="ManagedChannel"/>, 
    ///     shared between all components.
    /// </para>
    /// <para>
    ///     This channel is recommended to use for consumers, and the release is not required.
    /// </para>
    /// </summary>
    /// <returns>The shared <see cref="ManagedChannel"/>.</returns>
    ManagedChannel GetSharedChannel();

    /// <summary>
    /// <para>
    ///     Get the RabbitMQ managed channel, an object of type: <see cref="IModel"/>, from a pool.
    /// </para>
    /// <para>
    ///     Once completed the publication, the method <see cref="IDisposable.Dispose()"/> is required
    ///     to be called for others components can use the channel.
    /// </para>
    /// </summary>
    /// <returns>Task for async processing with the <see cref="IModel"/>.</returns>
    ManagedChannel GetPooledChannel();
}

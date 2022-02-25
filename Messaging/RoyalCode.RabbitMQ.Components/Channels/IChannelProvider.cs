using RabbitMQ.Client;
using System.Threading;
using System.Threading.Tasks;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// <para>
///     Component that handle the complexity to manage channel and connection and provides
///     the channel to communicate with the RabbitMQ.
/// </para>
/// </summary>
public interface IChannelProvider
{
    /// <summary>
    /// <para>
    ///     Get the RabbitMQ channel, an object of type: <see cref="IModel"/>, from a pool.
    /// </para>
    /// <para>
    ///     Once completed the publication, the method <see cref="ReturnPooledChannel(IModel)"/> is required
    ///     to be called for others components can use the channel.
    /// </para>
    /// </summary>
    /// <returns>Task for async processing with the <see cref="IModel"/>.</returns>
    Task<IModel> GetPooledChannelAsync(CancellationToken cancellationToken);

    /// <summary>
    /// <para>
    ///     Return the RabbitMQ chanell to the pool.
    /// </para>
    /// </summary>
    /// <param name="model">The used channel.</param>
    void ReturnPooledChannel(IModel model);

    /// <summary>
    /// Creates a new RabbitMQ channel, an object of type: <see cref="IModel"/>.
    /// </summary>
    /// <returns>A new <see cref="IModel"/>.</returns>
    IModel CreateChannel();

    /// <summary>
    /// Get the RabbitMQ channel, an object of type: <see cref="IModel"/>, shared between all components.
    /// </summary>
    /// <returns>The shared <see cref="IModel"/>.</returns>
    IModel GetSharedChannel();
}

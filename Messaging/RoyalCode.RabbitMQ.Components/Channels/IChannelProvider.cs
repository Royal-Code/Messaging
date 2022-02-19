using RabbitMQ.Client;
using System;

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
    /// Get the RabbitMQ channel, an object of type: <see cref="IModel"/>.
    /// </summary>
    IModel Channel { get; }

    /// <summary>
    /// <para>
    ///     Event raised when the channel was recreated.
    /// </para>
    /// <para>
    ///     This will occurs after the RabbitMQ connection are restablisher.
    /// </para>
    /// </summary>
    event EventHandler<IModel> ChannelRecreated;

    /// <summary>
    /// <para>
    ///     Event raised when the component are dispose.
    /// </para>
    /// </summary>
    event EventHandler ChannelDisposing;
}

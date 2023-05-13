using RabbitMQ.Client;
using System;

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
    /// <para>
    ///     Consume a channel provider.
    /// </para>
    /// </summary>
    /// <param name="name">The RabbitMQ cluster name.</param>
    /// <param name="consumer">The channel consumer.</param>
    /// <returns>A <ver cref="IDisposable"/> object to finalize the consumption.</returns>
    IChannelConsumption Consume(string name, IChannelConsumer consumer);
}

public sealed class ManagedChannel
{

}
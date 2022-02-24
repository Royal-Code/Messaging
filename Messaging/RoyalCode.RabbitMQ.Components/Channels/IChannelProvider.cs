﻿using RabbitMQ.Client;
using System;
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
    /// Get the RabbitMQ channel, an object of type: <see cref="IModel"/>.
    /// </summary>
    Task<IModel> GetChannelAsync(CancellationToken cancellationToken);
}

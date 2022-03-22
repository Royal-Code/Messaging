using System;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// <para>
///     A <ver cref="IDisposable"/> object to finalize the channel provider consumption.
/// </para>
/// </summary>
public interface IChannelConsumption : IDisposable { }
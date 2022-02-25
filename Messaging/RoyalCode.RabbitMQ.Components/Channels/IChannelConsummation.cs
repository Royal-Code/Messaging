using System;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// <para>
///     A <ver cref="IDisposable"/> object to finalize the channel provider consummation.
/// </para>
/// </summary>
public interface IChannelConsummation : IDisposable { }
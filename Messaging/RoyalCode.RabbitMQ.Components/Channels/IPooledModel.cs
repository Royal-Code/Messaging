using RabbitMQ.Client;
using System;

namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// <para>
///     Emcapsulates the <see cref="IModel"/>, obtained from the pool.
/// </para>
/// <para>
///     After use it, dispose should be called.
/// </para>
/// </summary>
public interface IPooledModel : IDisposable
{
    /// <summary>
    /// Get the RabbitMQ channel, an object of type: <see cref="IModel"/>.
    /// </summary>
    IModel Channel { get; }
}

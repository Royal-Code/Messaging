
namespace RoyalCode.RabbitMQ.Components.ObjectPool;

/// <summary>
/// <para>
///     Emcapsulates an object obtained from the pool.
/// </para>
/// <para>
///     After use the <see cref="Instance"/> (<typeparamref name="T"/>),
///     dispose this object for the <see cref="Instance"/> return to the pool.
/// </para>
/// </summary>
/// <typeparam name="T">The object type.</typeparam>
public interface IPooledObject<out T> : IDisposable
    where T : class
{
    /// <summary>
    /// The pooled instance.
    /// </summary>
    T Instance { get; }
}
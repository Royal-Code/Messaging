using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoyalCode.RabbitMQ.Components.ObjectPool;

/// <summary>
/// <para>
///     An object pool with limited number of instances and asynchronous delivery.
/// </para>
/// <para>
///     When an object is requested and the maximum number of instances is reached,
///     a Task is returned and queued, when an instance is released,
///     the Task result is assigned and the requester's processing will continue.
/// </para>
/// </summary>
/// <typeparam name="T">The object type.</typeparam>
public interface IAsyncObjectPool<T>
    where T : class
{
    /// <summary>
    /// <para>
    ///     Get a instance from the pool.
    /// </para>
    /// <para>
    ///     After use the pooled object, dispose him.
    /// </para>
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Task that returns the pooled object.</returns>
    Task<IPooledObject<T>> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// <para>
    ///     Return a pooled instance.
    /// </para>
    /// </summary>
    /// <param name="instace">The pooled instance.</param>
    /// <exception cref="InvalidOperationException">If the instance is not pooled.</exception>
    void Return(T instace);
}
using System;

namespace RoyalCode.RabbitMQ.Components.ObjectPool;

/// <summary>
/// Default implementation of <see cref="IPooledObject{T}"/>.
/// </summary>
/// <typeparam name="T">The object type.</typeparam>
public sealed class DefaultPooledObject<T> : IPooledObject<T>
    where T : class
{
    private readonly DefaultAsyncObjectPool<T> pool;
    private readonly T instance;

    private bool readyForUse;

    /// <summary>
    /// <para>
    ///     Creates a new pooled object, used by <see cref="DefaultAsyncObjectPool{T}"/>, and with the pooled instance.
    /// </para>
    /// </summary>
    /// <param name="pool">The pool that controls this pooled object.</param>
    /// <param name="instance">The instance.</param>
    public DefaultPooledObject(DefaultAsyncObjectPool<T> pool, T instance)
    {
        this.pool = pool;
        this.instance = instance;
    }

    /// <inheritdoc />
    public T Instance
    {
        get
        {
            if (!readyForUse)
                throw new InvalidOperationException("The pooled object is not ready to use");

            return instance;
        }
    }

    /// <summary>
    /// <para>
    ///     Internal, used by <see cref="DefaultAsyncObjectPool{T}"/> after initialize the object.
    /// </para>
    /// </summary>
    /// <exception cref="InvalidOperationException">If the object is in use.</exception>
    internal void SetReadyForUse()
    {
        if (readyForUse)
            throw new InvalidOperationException("The pooled object is already in use");
        
        readyForUse = true;  
    } 

    /// <inheritdoc />
    public void Dispose()
    {
        pool.Return(this);
        readyForUse = false;
    }
}
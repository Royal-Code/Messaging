using System;

namespace RoyalCode.RabbitMQ.Components.ObjectPool;

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
    public T Instace
    {
        get
        {
            if (!readyForUse)
                throw new InvalidOperationException("The pooled object is not ready to use");

            return instance;
        }
    }

    internal void SetReadyForUse()
    {
        if (readyForUse)
            throw new InvalidOperationException("The pooled object is already in use");
        
        readyForUse = true;  
    } 

    /// <inheritdoc />
    public void Dispose()
    {
        readyForUse = false;
        pool.Return(this);
    }
}
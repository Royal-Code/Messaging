using Microsoft.Extensions.ObjectPool;
using System.Collections.Concurrent;

namespace RoyalCode.RabbitMQ.Components.ObjectPool;

/// <summary>
/// Dotnet 8.0 class which is not available in the previous versions.
/// </summary>
/// <typeparam name="T">Object type</typeparam>
internal sealed class DisposableObjectPool<T> : ObjectPool<T>, IDisposable
    where T : class
{
    private volatile bool _isDisposed;

    private readonly Func<T> _createFunc;
    private readonly Func<T, bool> _returnFunc;
    private readonly int _maxCapacity;
    private int _numItems;

    private readonly ConcurrentQueue<T> _items = new();
    private T? _fastItem;

    /// <summary>
    /// Creates an instance of <see cref="DefaultObjectPool{T}"/>.
    /// </summary>
    /// <param name="policy">The pooling policy to use.</param>
    public DisposableObjectPool(IPooledObjectPolicy<T> policy)
        : this(policy, Environment.ProcessorCount * 2)
    { }

    /// <summary>
    /// Creates an instance of <see cref="DefaultObjectPool{T}"/>.
    /// </summary>
    /// <param name="policy">The pooling policy to use.</param>
    /// <param name="maximumRetained">The maximum number of objects to retain in the pool.</param>
    public DisposableObjectPool(IPooledObjectPolicy<T> policy, int maximumRetained)
    {
        // cache the target interface methods, to avoid interface lookup overhead
        _createFunc = policy.Create;
        _returnFunc = policy.Return;
        _maxCapacity = maximumRetained - 1;  // -1 to account for _fastItem
    }

    /// <inheritdoc />
    public override T Get()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }

        var item = _fastItem;
        if (item == null || Interlocked.CompareExchange(ref _fastItem, null, item) != item)
        {
            if (_items.TryDequeue(out item))
            {
                Interlocked.Decrement(ref _numItems);
                return item;
            }

            // no object available, so go get a brand new one
            return _createFunc();
        }

        return item;
    }

    /// <inheritdoc />
    public override void Return(T obj)
    {
        // When the pool is disposed or the obj is not returned to the pool, dispose it
        if (_isDisposed || !ReturnCore(obj))
        {
            DisposeItem(obj);
        }
    }

    /// <summary>
    /// Returns an object to the pool.
    /// </summary>
    /// <returns>true if the object was returned to the pool</returns>
    private bool ReturnCore(T obj)
    {
        if (!_returnFunc(obj))
        {
            // policy says to drop this object
            return false;
        }

        if (_fastItem != null || Interlocked.CompareExchange(ref _fastItem, obj, null) != null)
        {
            if (Interlocked.Increment(ref _numItems) <= _maxCapacity)
            {
                _items.Enqueue(obj);
                return true;
            }

            // no room, clean up the count and drop the object on the floor
            Interlocked.Decrement(ref _numItems);
            return false;
        }

        return true;
    }

    private static void DisposeItem(T? item)
    {
        if (item is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public void Dispose()
    {
        _isDisposed = true;

        DisposeItem(_fastItem);
        _fastItem = null;

        while (_items.TryDequeue(out var item))
        {
            DisposeItem(item);
        }
    }
}


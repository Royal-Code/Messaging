using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RoyalCode.RabbitMQ.Components.ObjectPool;

/// <summary>
/// <para>
///     Default implementation of <see cref="IAsyncObjectPool{T}"/>.
/// </para>
/// </summary>
/// <typeparam name="T">The object type.</typeparam>
public class DefaultAsyncObjectPool<T> : IAsyncObjectPool<T>
    where T : class
{
    private readonly ObjectPoolPolicy<T> policy;
    private readonly Object locker = new();
    private readonly Stack<DefaultPooledObject<T>> free = new();
    private readonly LinkedList<DefaultPooledObject<T>> inUse = new();
    private readonly Queue<Tuple<TaskCompletionSource<IPooledObject<T>>, CancellationToken>> scheduled = new();

    /// <summary>
    /// Creates a new pool with the policy.
    /// </summary>
    /// <param name="policy">The pool policy to create, initialize and finalize </param>
    public DefaultAsyncObjectPool(ObjectPoolPolicy<T> policy)
    {
        this.policy = policy;
    }

    /// <inheritdoc />
    public Task<IPooledObject<T>> GetAsync(CancellationToken cancellationToken)
    {
        DefaultPooledObject<T>? pooled;
        
        lock (locker)
        {
            if (!free.TryPop(out pooled))
            {
                if (!CanCreate())
                    return Schedule(cancellationToken);
                
                pooled = Create();
            }

            Initialize(pooled);
            inUse.AddLast(pooled);
        }

        return Task.FromResult<IPooledObject<T>>(pooled);
    }
    
    internal void Return(DefaultPooledObject<T> pooled)
    {
        lock (locker)
        {
            policy.Return(pooled.Instace);
            
            checkScheduled:
            if (scheduled.TryDequeue(out var scheduledItem))
            {
                if (scheduledItem.Item2.IsCancellationRequested)
                {
#if NETSTANDARD2_1
                    scheduledItem.Item1.SetCanceled();
#else
                    scheduledItem.Item1.SetCanceled(scheduledItem.Item2);
#endif
                    
                    goto checkScheduled;
                }

                Initialize(pooled);
                scheduledItem.Item1.TrySetResult(pooled);
            }
            else
            {
                inUse.Remove(pooled);
                free.Push(pooled);
            }
        }
    }

    private Task<IPooledObject<T>> Schedule(CancellationToken cancellationToken)
    {
        var task = new TaskCompletionSource<IPooledObject<T>>(TaskCreationOptions.RunContinuationsAsynchronously);
        scheduled.Enqueue(new Tuple<TaskCompletionSource<IPooledObject<T>>, CancellationToken>(task, cancellationToken));
        return task.Task;
    }

    private void Initialize(DefaultPooledObject<T> pooled)
    {
        policy.Initialize(pooled.Instace);
        pooled.SetReadyForUse();
    }

    private DefaultPooledObject<T> Create()
    {
        var instace = policy.Create();
        var pooled = new DefaultPooledObject<T>(this, instace);
        return pooled;
    }

    private bool CanCreate()
    {
        return inUse.Count < policy.MaxSize;
    }
}
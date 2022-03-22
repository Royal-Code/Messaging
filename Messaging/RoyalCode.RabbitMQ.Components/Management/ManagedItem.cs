using System;
using System.Collections.Generic;

namespace RoyalCode.RabbitMQ.Components.Management;

public abstract class AbstractManager<T> : IManager<T>
{
    private readonly Dictionary<string, ManagedItem<T>> pool = new();
    
    public IConsumption Consume(string name, IConsumer<T> consumer)
    {
        if (pool.TryGetValue(name, out var managed)) 
            return managed.AddConsumer(consumer);
        
        lock (pool)
        {
            if (pool.ContainsKey(name))
            {
                managed = pool[name];
            }
            else
            {
                managed = CreateManagedItem(name);
                pool[name] = managed;
            }
        }

        return managed.AddConsumer(consumer);
    }

    protected abstract ManagedItem<T> CreateManagedItem(string name);
}

public class ManagedItem<T>
{
    private readonly LinkedList<ManagedConsumption<T>> consumers = new();
    
    public virtual IConsumption AddConsumer(IConsumer<T>  consumer)
    {
        var managed = new ManagedConsumption<T>(this, consumer);
        lock (consumers)
        {
            consumers.AddLast(managed);
        }

        Consume(managed);
            
        return managed;
    }

    protected virtual void Consume(ManagedConsumption<T> managed) { }

    public virtual void RemoveConsumer(ManagedConsumption<T> managed)
    {
        lock (consumers)
        {
            consumers.Remove(managed);
        }
    }
    
    public virtual bool IsConnectionOpen { get; }
}

public class ManagedConsumption<T> : IConsumption
{
    private readonly ManagedItem<T> managedItem;
    private readonly IConsumer<T> consumer;
    private bool disposed;

    public ManagedConsumption(ManagedItem<T> managedItem, IConsumer<T> consumer)
    {
        this.consumer = consumer;
        this.managedItem = managedItem;
    }
    
    public bool IsConnectionOpen => managedItem.IsConnectionOpen;

    public void Consume(IProvider<T> provider) => consumer.Consume(provider);

    public void Reconnected(bool autoRecovered) => consumer.Reconnected(autoRecovered);

    public void Closed() => consumer.Closed();
    
    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        managedItem.RemoveConsumer(this);
    }
}
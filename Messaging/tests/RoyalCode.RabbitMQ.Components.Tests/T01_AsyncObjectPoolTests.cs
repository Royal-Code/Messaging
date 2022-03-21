using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoyalCode.RabbitMQ.Components.ObjectPool;
using Xunit;

namespace RoyalCode.RabbitMQ.Components.Tests;

public class T01_AsyncObjectPoolTests
{
    [Fact]
    public async Task T01_GetSingleObject()
    {
        var policy = new ObjectPoolPolicy<ObjectPoolTestType>(1, 
            ObjectPoolTestType.Create, ObjectPoolTestType.Initialize, ObjectPoolTestType.Return);
        
        var pool = new DefaultAsyncObjectPool<ObjectPoolTestType>(policy);
        var pooled = await pool.GetAsync();
        
        Assert.NotNull(pooled);
        Assert.NotNull(pooled.Instace);
        Assert.Equal(1, pool.CountObjectsInUse());
        
        pooled.Dispose();
        Assert.Equal(0, pool.CountObjectsInUse());
    }
    
    [Fact]
    public async Task T02_SameObjectNotSamePooled()
    {
        var policy = new ObjectPoolPolicy<ObjectPoolTestType>(1, 
            ObjectPoolTestType.Create, ObjectPoolTestType.Initialize, ObjectPoolTestType.Return);
        
        var pool = new DefaultAsyncObjectPool<ObjectPoolTestType>(policy);
        var pooled1 = await pool.GetAsync();
        
        Assert.NotNull(pooled1);
        Assert.NotNull(pooled1.Instace);
        var first = pooled1.Instace;
        pooled1.Dispose();
        
        var pooled2 = await pool.GetAsync();
        Assert.NotNull(pooled2);
        Assert.NotNull(pooled2.Instace);
        var second = pooled2.Instace;
        
        Assert.Same(first, second);
        Assert.NotSame(pooled1, pooled2);
    }
    
    [Fact]
    public async Task T03_ManyObjects()
    {
        var policy = new ObjectPoolPolicy<ObjectPoolTestType>(10, 
            ObjectPoolTestType.Create, ObjectPoolTestType.Initialize, ObjectPoolTestType.Return);
        
        var pool = new DefaultAsyncObjectPool<ObjectPoolTestType>(policy);

        List<IPooledObject<ObjectPoolTestType>> polleds = new();
        for (int i = 0; i < 10; i++)
        {
            polleds.Add(await pool.GetAsync());    
        }
        
        Assert.Equal(10, polleds.Count);
        Assert.Equal(10, pool.CountObjectsInUse());
        
        polleds.ForEach(p => p.Dispose());
        
        Assert.Equal(0, pool.CountObjectsInUse());
    }

    [Fact]
    public async Task T04_Waiting()
    {
        var policy = new ObjectPoolPolicy<ObjectPoolTestType>(1, 
            ObjectPoolTestType.Create, ObjectPoolTestType.Initialize, ObjectPoolTestType.Return);
        
        var pool = new DefaultAsyncObjectPool<ObjectPoolTestType>(policy);
        
        var pooled1 = await pool.GetAsync();
        var first = pooled1.Instace;
        _ = Task.Run(async () =>
        {
            await Task.Delay(2000);
            pooled1.Dispose();
        });

        var pooled2Task = pool.GetAsync();
        
        Assert.Equal(1, pool.CountObjectsInUse());

        var pooled2 = await pooled2Task;
        var second = pooled2.Instace;
        
        Assert.Equal(1, pool.CountObjectsInUse());
        Assert.Same(first, second);
        
        pooled2.Dispose();
        Assert.Equal(0, pool.CountObjectsInUse());
    }

    [Fact]
    public async Task T05_InitializeAndReturn()
    {
        var policy = new ObjectPoolPolicy<ObjectPoolTestType>(1, 
            ObjectPoolTestType.Create, ObjectPoolTestType.Initialize, ObjectPoolTestType.Return);
        
        var pool = new DefaultAsyncObjectPool<ObjectPoolTestType>(policy);
        var pooled = await pool.GetAsync();

        var instance = pooled.Instace;
        Assert.True(instance.Initialized);
        Assert.False(instance.Returned);
        
        pooled.Dispose();
        
        Assert.True(instance.Returned);
    }

    [Fact]
    public async Task T06_CanNotAccessDisposedPooledObject()
    {
        var policy = new ObjectPoolPolicy<ObjectPoolTestType>(1, 
            ObjectPoolTestType.Create, ObjectPoolTestType.Initialize, ObjectPoolTestType.Return);
        
        var pool = new DefaultAsyncObjectPool<ObjectPoolTestType>(policy);
        var pooled = await pool.GetAsync();
        pooled.Dispose();

        Assert.Throws<InvalidOperationException>(() => pooled.Instace);
    }
}

public class ObjectPoolTestType
{
    public static ObjectPoolTestType Create() => new();

    public static void Initialize(ObjectPoolTestType type)
    {
        type.Initialized = true;
    }

    public static void Return(ObjectPoolTestType type)
    {
        type.Returned = true;
    }

    public bool Initialized { get; private set; }

    public bool Returned { get; private set; }
}
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RoyalCode.RabbitMQ.Components.Channels;
using Xunit;

namespace RoyalCode.RabbitMQ.Components.Tests;

public class T03_Channels
{
    [Fact]
    public void T01_GetChannelManager()
    {
        var sp = Container.Prepare();

        var cm = sp.GetService<IChannelManager>();
        Assert.NotNull(cm);
    }
    
    [Fact]
    public void T02_ConsumeFromChannelManager()
    {
        var sp = Container.Prepare();

        var cm = sp.GetService<IChannelManager>();

        var consumer = new TestChannelConsumer();
        cm!.Consume("test", consumer);
        
        Assert.NotNull(consumer.ChannelProvider);
    }

    [Fact]
    public void T03_GetSharedModel()
    {
        var sp = Container.Prepare();

        var cm = sp.GetService<IChannelManager>();
        var consumer = new TestChannelConsumer();
        cm!.Consume("test", consumer);
        var provider = consumer.ChannelProvider!;

        var first = provider.GetSharedChannel();
        Assert.NotNull(first);
        
        var second = provider.GetSharedChannel();
        Assert.NotNull(second);
        
        Assert.Same(first, second);
    }
    
    [Fact]
    public void T04_CreateModels()
    {
        var sp = Container.Prepare();

        var cm = sp.GetService<IChannelManager>();
        var consumer = new TestChannelConsumer();
        cm!.Consume("test", consumer);
        var provider = consumer.ChannelProvider!;

        var first = provider.CreateChannel();
        var second = provider.CreateChannel();
        
        Assert.NotSame(first, second);
    }

    [Fact]
    public async Task T05_GetPooledModels_NotSame()
    {
        var sp = Container.Prepare();

        var cm = sp.GetService<IChannelManager>();
        var consumer = new TestChannelConsumer();
        cm!.Consume("test", consumer);
        var provider = consumer.ChannelProvider!;

        var first = await provider.GetPooledChannelAsync();
        var second = await provider.GetPooledChannelAsync();
        
        Assert.NotSame(first, second);
    }
    
    [Fact]
    public async Task T06_GetPooledModels_Same()
    {
        var sp = Container.Prepare();

        var cm = sp.GetService<IChannelManager>();
        var consumer = new TestChannelConsumer();
        cm!.Consume("test", consumer);
        var provider = consumer.ChannelProvider!;

        var first = await provider.GetPooledChannelAsync();
        provider.ReturnPooledChannel(first);
        
        var second = await provider.GetPooledChannelAsync();
        provider.ReturnPooledChannel(second);
        
        Assert.Same(first, second);
    }
}

public class TestChannelConsumer : IChannelConsumer
{
    public IChannelProvider? ChannelProvider { get; private set; }
    
    public void Consume(IChannelProvider provider)
    {
        ChannelProvider = provider;
    }

    public void ConnectionClosed() { }

    public void ConnectionRecovered(bool autorecovered) { }
}
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RoyalCode.RabbitMQ.Components.Channels;
using Xunit;

namespace RoyalCode.RabbitMQ.Components.Tests;

public class T03_ChannelTests
{
    [Fact]
    public void T01_1_GetChannelManager_FromContainer_MustReturnNull()
    {
        var sp = Container.Prepare();

        var cm = sp.GetService<IChannelManager>();
        Assert.Null(cm);
    }

    [Fact]
    public void T01_2_GetChannelManagerFactory_FromContainer_MustReturnValue()
    {
        var sp = Container.Prepare();

        var f = sp.GetService<IChannelManagerFactory>();
        Assert.NotNull(f);
    }

    [Fact]
    public void T01_3_GetChannelManager_FromFactory_MustReturnValue()
    {
        var sp = Container.Prepare();

        var f = sp.GetService<IChannelManagerFactory>();
        Assert.NotNull(f);

        var cm = f!.GetChannelManager("test");
        Assert.NotNull(cm);
    }

    [Fact]
    public void T02_ConsumeFromChannelManager()
    {
        var sp = Container.Prepare();

        var factory = sp.GetService<IChannelManagerFactory>();
        var cm = factory!.GetChannelManager("test");
        var managed = cm.CreateChannel();

        var consumer = new TestChannelConsumer();
        managed.Consume(consumer);
        
        Assert.NotNull(consumer.Model);
    }

    [Fact]
    public void T03_GetSharedModel()
    {
        var sp = Container.Prepare();

        var factory = sp.GetService<IChannelManagerFactory>();
        var cm = factory!.GetChannelManager("test");

        var first = cm.GetSharedChannel();
        Assert.NotNull(first);
        
        var second = cm.GetSharedChannel();
        Assert.NotNull(second);
        
        Assert.Same(first, second);
    }
    
    [Fact]
    public void T04_CreateModels()
    {
        var sp = Container.Prepare();

        var factory = sp.GetService<IChannelManagerFactory>();
        var cm = factory!.GetChannelManager("test");

        var first = cm.CreateChannel();
        Assert.NotNull(first);

        var second = cm.CreateChannel();
        Assert.NotNull(second);

        Assert.NotSame(first, second);
    }

    [Fact]
    public void T05_GetPooledModels_NotSame()
    {
        var sp = Container.Prepare();

        var factory = sp.GetService<IChannelManagerFactory>();
        var cm = factory!.GetChannelManager("test");
        
        var first = cm.GetPooledChannel();
        var second = cm.GetPooledChannel();

        Assert.NotSame(first, second);
    }
    
    [Fact]
    public void T06_GetPooledModels_Same()
    {
        var sp = Container.Prepare();

        var factory = sp.GetService<IChannelManagerFactory>();
        var cm = factory!.GetChannelManager("test");

        var managed = cm.GetPooledChannel();
        var first = managed.Channel;

        managed.Dispose();

        managed = cm.GetPooledChannel();
        var second = managed.Channel;

        managed.Dispose();

        Assert.Same(first, second);
    }
}
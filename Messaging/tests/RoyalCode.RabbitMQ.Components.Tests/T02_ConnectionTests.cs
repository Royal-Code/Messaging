using Microsoft.Extensions.DependencyInjection;
using RoyalCode.RabbitMQ.Components.Connections;
using Xunit;

namespace RoyalCode.RabbitMQ.Components.Tests;

public class T02_ConnectionTests
{
    [Fact]
    public void T01_Connect()
    {
        var sp = Container.Prepare();

        var cm = sp.GetService<ConnectionManager>();

        Assert.NotNull(cm);

        var consumer = new TestConnectionConsumer();
        cm!.Consume("test", consumer);

        Assert.NotNull(consumer.Connection);
    }
    
    [Fact]
    public void T02_SameConnect()
    {
        var sp = Container.Prepare();

        var cm = sp.GetService<ConnectionManager>();

        Assert.NotNull(cm);

        var consumer = new TestConnectionConsumer();
        cm!.Consume("test", consumer);
        var first = consumer.Connection;
        consumer.Disposing();
        
        consumer = new TestConnectionConsumer();
        cm.Consume("test", consumer);
        var second = consumer.Connection;
        consumer.Disposing();

        Assert.True(first!.IsOpen);
        Assert.Same(first, second);
    }
}
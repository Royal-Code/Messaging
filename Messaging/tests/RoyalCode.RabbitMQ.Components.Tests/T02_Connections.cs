using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Connections;
using System;
using Xunit;

namespace RoyalCode.RabbitMQ.Components.Tests;

public class T02_Connections
{
    [Fact]
    public void T01_Connect()
    {
        var sp = Container.Prepare();

        var cm = sp.GetService<ConnectionManager>();

        Assert.NotNull(cm);

        var consumer = new TestConnectionConsumer();
        cm!.Consume("test", consumer);

        Assert.NotNull(consumer.ConnectionProvider);
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
        consumer.Dispose();
        
        consumer = new TestConnectionConsumer();
        cm!.Consume("test", consumer);
        var second = consumer.Connection;
        consumer.Dispose();
        
        Assert.True(first.IsOpen);
        Assert.Same(first, second);
    }

    private class TestConnectionConsumer : IConnectionConsumer
    {
        public IConnection? Connection => ConnectionProvider?.Connection;

        public IConnectionProvider? ConnectionProvider { get; private set; }

        public void Closed() { }

        public void Consume(IConnectionProvider connectionProvider)
        {
            ConnectionProvider = connectionProvider;
        }

        public void Dispose()
        {
            ConnectionProvider?.Dispose();
            ConnectionProvider = null;
        }

        public void Reload(bool autorecovered) { }
    }
}

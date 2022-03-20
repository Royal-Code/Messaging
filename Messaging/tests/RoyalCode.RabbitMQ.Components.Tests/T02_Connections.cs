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
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging(builder => builder.AddConsole());

        services.AddRabbitMQComponents();
        services.ConfigureRabbitMQConnection("test", options =>
        {
            options.AddConnectionStringName("test");
        });

        var sp = services.BuildServiceProvider();

        var cm = sp.GetService<ConnectionManager>();

        Assert.NotNull(cm);

        var consumer = new TestConnectionConsumer();
        cm!.Consume("test", consumer);

        Assert.NotNull(consumer.ConnectionProvider);
        Assert.NotNull(consumer.Connection);
    }

    private class TestConnectionConsumer : IConnectionConsumer
    {
        public IConnection Connection => ConnectionProvider.Connection;

        public IConnectionProvider ConnectionProvider { get; private set; }

        public void Closed() { }

        public void Consume(IConnectionProvider connection)
        {
            ConnectionProvider = connection;
        }

        public void Dispose() { }

        public void Reload(bool autorecovered) { }
    }
}

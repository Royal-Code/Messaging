using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Connections;

namespace RoyalCode.RabbitMQ.Components.Tests;

public class TestConnectionConsumer : IConnectionConsumer
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
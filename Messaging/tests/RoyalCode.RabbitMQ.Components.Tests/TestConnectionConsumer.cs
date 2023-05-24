using RabbitMQ.Client;
using RoyalCode.RabbitMQ.Components.Connections;

namespace RoyalCode.RabbitMQ.Components.Tests;

public class TestConnectionConsumer : IConnectionConsumer
{
    public IConnection? Connection { get; private set; }

    public void Consume(IConnection connection)
    {
        Connection = connection;
    }

    public void Disposing()
    {
        Connection = null;
    }

    public void Reloaded(IConnection connection, bool autorecovered)
    {
        Connection = connection;
    }
}
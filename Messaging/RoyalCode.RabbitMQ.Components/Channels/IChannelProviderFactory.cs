namespace RoyalCode.RabbitMQ.Components.Channels;

/// <summary>
/// <para>
///     The factory to create <see cref="IChannelProvider"/> and 
/// </para>
/// </summary>
public interface IChannelProviderFactory
{
    IChannelProvider GetSingleton(string name);

    IChannelProvider Create(string name); // Irá requerer o tipo? Como retorna o IModel,
                                          // ele pode ser configurado posteriormente, por que chama o método.

    IPooledChannelProvider GetPooled(string name); // requer o tipo?
}

public interface IChannelManager
{

    void ConsumeSingletonChannel(string name, IChannelConsumer consumer);


}

public interface IChannelConsumer
{

    void Consume(IChannelProvider provider);

    void ConnectionClosed();

    void ConnectionRecovered(bool autorecovered);
}
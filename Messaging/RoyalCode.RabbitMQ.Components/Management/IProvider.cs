namespace RoyalCode.RabbitMQ.Components.Management;

public interface IProvider<T>
{
    T Item { get; }
    
    string ConnectionName { get; }
    
    bool IsConnectionOpen { get; }
}
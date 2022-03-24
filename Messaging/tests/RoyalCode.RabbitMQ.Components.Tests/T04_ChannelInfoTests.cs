using System.Linq;
using RabbitMQ.Client.Exceptions;
using RoyalCode.RabbitMQ.Components.Communication;
using Xunit;

namespace RoyalCode.RabbitMQ.Components.Tests;

public class T04_ChannelInfoTests
{
    [Fact]
    public void T01_TempQueue()
    {
        using var factory = new ModelFactory();
        var model = factory.ChannelProvider.CreateChannel();
        
        var info = ChannelInfo.TemporaryQueue("Test_Temp_Queue");
        var ok = info.GetConsumerQueue(model);
        
        Assert.Equal("Test_Temp_Queue", ok.QueueName);

        model.QueueDeclarePassive("Test_Temp_Queue");
        
        model.Dispose();
        factory.ConnectionProvider.Connection.Close(1, "Closing for tests purposes");
        factory.WaitForConnected();
        
        model = factory.ChannelProvider.CreateChannel();
        var ex = Assert.Throws<OperationInterruptedException>(() => model.QueueDeclarePassive("Test_Temp_Queue"));
        Assert.Contains("404", ex.Message);
    }

    [Fact]
    public void T02_PersistentQueue()
    {
        using var factory = new ModelFactory();
        var model = factory.ChannelProvider.CreateChannel();
        
        var info = ChannelInfo.PersistentQueue("Test_Persistent_Queue");
        var ok = info.GetConsumerQueue(model);
        
        model.QueueDeclarePassive("Test_Persistent_Queue");
        
        model.Dispose();
        factory.ConnectionProvider.Connection.Close(1, "Closing for tests purposes");
        factory.WaitForConnected();
        
        model = factory.ChannelProvider.CreateChannel();
        model.QueueDeclarePassive("Test_Persistent_Queue");
        model.QueueDelete("Test_Persistent_Queue", true, true);
    }

    [Fact]
    public void T03_PersistentQueueWithDeadLetter()
    {
        using var factory = new ModelFactory();
        var model = factory.ChannelProvider.CreateChannel();
        
        var info = ChannelInfo.QueueWithDeadLetter("Test_Persistent_Queue_With_Deadletter");
        var ok = info.GetConsumerQueue(model);

        Assert.True(info.Queue!.DeadLetter.Active);

        var args = info.Queue.CreateArguments();
        Assert.Contains(args.Keys, v => v == "x-dead-letter-exchange");
        Assert.Contains(args.Keys, v => v == "x-dead-letter-routing-key");
        
        model.QueueDeclarePassive("Test_Persistent_Queue_With_Deadletter");
        model.QueueDelete("Test_Persistent_Queue_With_Deadletter", true, true);
    }
    
    
}
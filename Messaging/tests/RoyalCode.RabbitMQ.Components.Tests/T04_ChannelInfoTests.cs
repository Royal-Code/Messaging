using System;
using System.Threading.Tasks;
using RabbitMQ.Client.Exceptions;
using RoyalCode.RabbitMQ.Components.Communication;
using RoyalCode.RabbitMQ.Components.Declarations;
using Xunit;

namespace RoyalCode.RabbitMQ.Components.Tests;

public class T04_ChannelInfoTests
{
    [Fact]
    public async Task T01_TempQueue()
    {
        using var factory = new ModelFactory();
        var model = factory.ChannelProvider.CreateChannel();
        
        var info = ChannelInfo.TemporaryQueue("Test_Temp_Queue");
        var ok = info.GetConsumerQueue(model);
        
        Assert.Equal("Test_Temp_Queue", ok.QueueName);

        model.QueueDeclarePassive("Test_Temp_Queue");
        
        model.Dispose();
        await Task.Delay(30);

        factory.ConnectionProvider.Connection.Close(1, "Closing for tests purposes");
        factory.WaitForConnected();
        
        model = factory.ChannelProvider.CreateChannel();
        var ex = Assert.Throws<OperationInterruptedException>(() => model.QueueDeclarePassive("Test_Temp_Queue"));
        Assert.Contains("404", ex.Message);
    }

    [Fact]
    public async Task T02_PersistentQueueAsync()
    {
        using var factory = new ModelFactory();
        var model = factory.ChannelProvider.CreateChannel();
        
        var info = ChannelInfo.PersistentQueue("Test_Persistent_Queue");
        var ok = info.GetConsumerQueue(model);
        
        model.QueueDeclarePassive("Test_Persistent_Queue");
        
        model.Dispose();
        await Task.Delay(30);

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

    [Fact]
    public void T04_ThrowWhenGetConsumerQueueForExchangeWithoutQueue()
    {
        using var factory = new ModelFactory();
        var model = factory.ChannelProvider.CreateChannel();

        var info = ChannelInfo.FanoutExchange("Test_FanoutExchage");
        Assert.Throws<InvalidOperationException>(() => info.GetConsumerQueue(model));
    }

    [Fact]
    public void T05_FanoutExchange_DeclareForPublish()
    {
        using var factory = new ModelFactory();
        var model = factory.ChannelProvider.CreateChannel();

        var info = ChannelInfo.FanoutExchange("Test_FanoutExchage_ForPublish");
        var address = info.GetPublicationAddress(model);

        model.ExchangeDeclarePassive("Test_FanoutExchage_ForPublish");
        model.ExchangeDelete("Test_FanoutExchage_ForPublish", false);
    }

    [Fact]
    public async Task T06_FanoutExchange_DeclareForConsumeAsync()
    {
        using var factory = new ModelFactory();
        var model = factory.ChannelProvider.CreateChannel();

        var info = ChannelInfo.TemporaryQueue()
            .BindToFanout("Test_FanoutExchage_ForConsume");
        var ok = info.GetConsumerQueue(model);

        model.QueueDeclarePassive(ok.QueueName);
        model.ExchangeDeclarePassive("Test_FanoutExchage_ForConsume");

        var ex = Assert.ThrowsAny<Exception>(() => model.ExchangeDelete("Test_FanoutExchage_ForConsume", true));
        Assert.Contains("406", ex.Message);

        model.Dispose();
        await Task.Delay(30);

        model = factory.ChannelProvider.CreateChannel();
        model.ExchangeDelete("Test_FanoutExchage_ForConsume", false);
    }

    [Fact]
    public void T07_RouteExchange_DeclareForPublish()
    {
        using var factory = new ModelFactory();
        var model = factory.ChannelProvider.CreateChannel();

        var info = ChannelInfo.RouteExchange("Test_RouteExchange_ForPublish", "Test.Route");
        var address = info.GetPublicationAddress(model);

        model.ExchangeDeclarePassive("Test_RouteExchange_ForPublish");
        model.ExchangeDelete("Test_RouteExchange_ForPublish", false);
    }

    [Fact]
    public void T08_TopicExchange_DeclareForPublish()
    {
        using var factory = new ModelFactory();
        var model = factory.ChannelProvider.CreateChannel();

        var info = ChannelInfo.TopicExchange("Test_TopicExchange_ForPublish", "Test.Topic");
        var address = info.GetPublicationAddress(model);

        model.ExchangeDeclarePassive("Test_TopicExchange_ForPublish");
        model.ExchangeDelete("Test_TopicExchange_ForPublish", false);
    }
}
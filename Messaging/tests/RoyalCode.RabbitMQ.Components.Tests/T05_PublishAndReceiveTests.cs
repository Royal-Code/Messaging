
using RoyalCode.RabbitMQ.Components.Communication;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RoyalCode.RabbitMQ.Components.Tests;

public class T05_PublishAndReceiveTests
{
    [Fact]
    public async Task T01_PublishToFanout()
    {
        using var factory = new ModelFactory();
        var info = ChannelInfo.FanoutExchange("Test_PublishToFanout");

        var publisher = new Publisher(
            info,
            factory.ChannelManager,
            "test",
            Channels.ChannelStrategy.Pooled,
            factory.CreateLogger<Publisher>());

        var message = new PublicationMessage(nameof(T01_PublishToFanout));

        await publisher.Publish(message);

        factory.Model.ExchangeDelete("Test_PublishToFanout", false);
    }

    [Fact]
    public void T02_CanNotPublishUsingSharedStrategy()
    {
        using var factory = new ModelFactory();
        var info = ChannelInfo.FanoutExchange("Test_CanNotPublishUsingSharedStrategy");

        Assert.Throws<CommunicationException>(() =>
        {
            _ = new Publisher(
                info,
                factory.ChannelManager,
                "test",
                Channels.ChannelStrategy.Shared,
                factory.CreateLogger<Publisher>());
        });
        
        factory.Model.ExchangeDelete("Test_CanNotPublishUsingSharedStrategy", false);
    }

    [Fact]
    public async Task T03_PublishToQueue()
    {
        using var factory = new ModelFactory();
        var info = ChannelInfo.TemporaryQueue();

        var publisher = new Publisher(
            info,
            factory.ChannelManager,
            "test",
            Channels.ChannelStrategy.Pooled,
            factory.CreateLogger<Publisher>());

        var message = new PublicationMessage(nameof(T03_PublishToQueue));

        await publisher.Publish(message);
    }
}

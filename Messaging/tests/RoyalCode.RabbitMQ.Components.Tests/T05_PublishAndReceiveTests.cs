using RabbitMQ.Client.Events;
using RoyalCode.RabbitMQ.Components.Communication;
using RoyalCode.RabbitMQ.Components.Declarations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        factory.Model.ExchangeDelete(info.Name, false);
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
        
        factory.Model.ExchangeDelete(info.Name, false);
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

    [Fact]
    public async Task T04_PublishToQueueAndReceive()
    {
        using var factory = new ModelFactory();
        var info = ChannelInfo.TemporaryQueue();

        string? msg = null;
        void Consumer(object? sender, BasicDeliverEventArgs e) {
            msg = Encoding.UTF8.GetString(e.Body.Span);
        };

        var listener = new MessageListener(Consumer);
        var receiver = new Receiver(
            info,
            factory.ChannelManager,
            "test",
            Channels.ChannelStrategy.Shared,
            factory.CreateLogger<Receiver>());

        receiver.Listen(listener);

        var publisher = new Publisher(
            info,
            factory.ChannelManager,
            "test",
            Channels.ChannelStrategy.Pooled,
            factory.CreateLogger<Publisher>());

        var message = new PublicationMessage(nameof(T04_PublishToQueueAndReceive));

        await publisher.Publish(message);

        await Task.Delay(30);

        Assert.Equal(nameof(T04_PublishToQueueAndReceive), msg);
    }

    [Fact]
    public async Task T05_PublishToFanoutAndReceive()
    {
        using var factory = new ModelFactory();

        var exchange = ChannelInfo.FanoutExchange("PublishToFanoutAndReceive");
        var queue = ChannelInfo.TemporaryQueue().BindTo(exchange);

        string? msg = null;
        void Consumer(object? sender, BasicDeliverEventArgs e)
        {
            msg = Encoding.UTF8.GetString(e.Body.Span);
        };

        var listener = new MessageListener(Consumer);
        var receiver = new Receiver(
            queue,
            factory.ChannelManager,
            "test",
            Channels.ChannelStrategy.Shared,
            factory.CreateLogger<Receiver>());

        receiver.Listen(listener);

        var publisher = new Publisher(
            exchange,
            factory.ChannelManager,
            "test",
            Channels.ChannelStrategy.Pooled,
            factory.CreateLogger<Publisher>());

        var message = new PublicationMessage(nameof(T05_PublishToFanoutAndReceive));

        await publisher.Publish(message);

        await Task.Delay(30);

        Assert.Equal(nameof(T05_PublishToFanoutAndReceive), msg);

        factory.Model.ExchangeDelete(exchange.Name, false);
    }

    [Fact]
    public async Task T06_PublishToRouteAndReceive1()
    {
        using var factory = new ModelFactory();

        var exchange = ChannelInfo.RouteExchange("PublishToRouteAndReceive1");
        var queue1 = ChannelInfo.TemporaryQueue().BindTo(exchange, "orange");

        List<Tuple<string, string>> msgs = new();
        void Consumer(object? sender, BasicDeliverEventArgs e)
        {
            msgs.Add(new Tuple<string, string>(e.RoutingKey, Encoding.UTF8.GetString(e.Body.Span)));
        };

        var listener = new MessageListener(Consumer);
        var receiver = new Receiver(
            queue1,
            factory.ChannelManager,
            "test",
            Channels.ChannelStrategy.Shared,
            factory.CreateLogger<Receiver>());

        receiver.Listen(listener);

        var publisher = new Publisher(
            exchange,
            factory.ChannelManager,
            "test",
            Channels.ChannelStrategy.Pooled,
            factory.CreateLogger<Publisher>());

        var message = new PublicationMessage(nameof(T06_PublishToRouteAndReceive1))
        {
            RoutingKey = "orange"
        };
        await publisher.Publish(message);

        message = new PublicationMessage(nameof(T06_PublishToRouteAndReceive1))
        {
            RoutingKey = "black"
        };
        await publisher.Publish(message);

        await Task.Delay(30);

        Assert.Single(msgs);

        var first = msgs[0];
        Assert.Equal("orange", first.Item1);
        Assert.Equal(nameof(T06_PublishToRouteAndReceive1), first.Item2);

        factory.Model.ExchangeDelete(exchange.Name, false);
    }

    [Fact]
    public async Task T07_PublishToRouteAndReceive2()
    {
        using var factory = new ModelFactory();

        var exchange = ChannelInfo.RouteExchange("PublishToRouteAndReceive2");
        var queue1 = ChannelInfo.TemporaryQueue().BindTo(exchange, "orange");
        var queue2 = ChannelInfo.TemporaryQueue().BindTo(exchange, "black", "green");

        List<Tuple<string, string>> msgs = new();
        void Consumer(object? sender, BasicDeliverEventArgs e)
        {
            msgs.Add(new Tuple<string, string>(e.RoutingKey, Encoding.UTF8.GetString(e.Body.Span)));
        };

        var listener = new MessageListener(Consumer);

        var receiver1 = new Receiver(
            queue1,
            factory.ChannelManager,
            "test",
            Channels.ChannelStrategy.Shared,
            factory.CreateLogger<Receiver>());
        receiver1.Listen(listener);

        var receiver2 = new Receiver(
            queue2,
            factory.ChannelManager,
            "test",
            Channels.ChannelStrategy.Shared,
            factory.CreateLogger<Receiver>());
        receiver2.Listen(listener);

        var publisher = new Publisher(
            exchange,
            factory.ChannelManager,
            "test",
            Channels.ChannelStrategy.Pooled,
            factory.CreateLogger<Publisher>());

        var message = new PublicationMessage(nameof(T07_PublishToRouteAndReceive2))
        {
            RoutingKey = "orange"
        };
        await publisher.Publish(message);

        message = new PublicationMessage(nameof(T07_PublishToRouteAndReceive2))
        {
            RoutingKey = "black"
        };
        await publisher.Publish(message);

        message = new PublicationMessage(nameof(T07_PublishToRouteAndReceive2))
        {
            RoutingKey = "green"
        };
        await publisher.Publish(message);

        message = new PublicationMessage(nameof(T07_PublishToRouteAndReceive2))
        {
            RoutingKey = "white"
        };
        await publisher.Publish(message);

        await Task.Delay(30);

        Assert.Equal(3, msgs.Count);

        var first = msgs[0];
        Assert.Equal("orange", first.Item1);
        Assert.Equal(nameof(T07_PublishToRouteAndReceive2), first.Item2);

        var second = msgs[1];
        Assert.Equal("black", second.Item1);
        Assert.Equal(nameof(T07_PublishToRouteAndReceive2), second.Item2);

        var third = msgs[2];
        Assert.Equal("green", third.Item1);
        Assert.Equal(nameof(T07_PublishToRouteAndReceive2), third.Item2);

        factory.Model.ExchangeDelete(exchange.Name, false);
    }

    [Fact]
    public async Task T08_PublishToTopicAndReceive()
    {
        using var factory = new ModelFactory();

        var exchange = ChannelInfo.TopicExchange("PublishToTopicAndReceive");
        var queue1 = ChannelInfo.TemporaryQueue().BindTo(exchange, "*.orange.*");
        var queue2 = ChannelInfo.TemporaryQueue().BindTo(exchange, "*.*.rabbit", "lazy.#");

        ConcurrentStack<Tuple<string, string>> msgs = new();
        void Consumer(object? sender, BasicDeliverEventArgs e)
        {
            msgs.Push(new Tuple<string, string>(e.RoutingKey, Encoding.UTF8.GetString(e.Body.Span)));
        };

        var listener = new MessageListener(Consumer);

        var receiver1 = new Receiver(
            queue1,
            factory.ChannelManager,
            "test",
            Channels.ChannelStrategy.Shared,
            factory.CreateLogger<Receiver>());
        receiver1.Listen(listener);

        var receiver2 = new Receiver(
            queue2,
            factory.ChannelManager,
            "test",
            Channels.ChannelStrategy.Shared,
            factory.CreateLogger<Receiver>());
        receiver2.Listen(listener);

        var publisher = new Publisher(
            exchange,
            factory.ChannelManager,
            "test",
            Channels.ChannelStrategy.Pooled,
            factory.CreateLogger<Publisher>());

        var message = new PublicationMessage(nameof(T08_PublishToTopicAndReceive))
        {
            RoutingKey = "quick.orange.rabbit"
        };
        await publisher.Publish(message);

        message = new PublicationMessage(nameof(T08_PublishToTopicAndReceive))
        {
            RoutingKey = "lazy.orange.elephant"
        };
        await publisher.Publish(message);

        message = new PublicationMessage(nameof(T08_PublishToTopicAndReceive))
        {
            RoutingKey = "quick.orange.fox"
        };
        await publisher.Publish(message);

        message = new PublicationMessage(nameof(T08_PublishToTopicAndReceive))
        {
            RoutingKey = "lazy.brown.fox"
        };
        await publisher.Publish(message);

        message = new PublicationMessage(nameof(T08_PublishToTopicAndReceive))
        {
            RoutingKey = "lazy.pink.rabbit"
        };
        await publisher.Publish(message);

        message = new PublicationMessage(nameof(T08_PublishToTopicAndReceive))
        {
            RoutingKey = "quick.brown.fox"
        };
        await publisher.Publish(message);

        message = new PublicationMessage(nameof(T08_PublishToTopicAndReceive))
        {
            RoutingKey = "quick.orange.male.rabbit"
        };
        await publisher.Publish(message);

        message = new PublicationMessage(nameof(T08_PublishToTopicAndReceive))
        {
            RoutingKey = "lazy.orange.male.rabbit"
        };
        await publisher.Publish(message);

        await Task.Delay(30);

        string[] expected = new[] 
        { 
            "quick.orange.rabbit", "quick.orange.rabbit",
            "lazy.orange.elephant", "lazy.orange.elephant",
            "quick.orange.fox",
            "lazy.brown.fox", 
            "lazy.pink.rabbit",
            "lazy.orange.male.rabbit"
        };

        Assert.Equal(expected.Length, msgs.Count);

        var messages = msgs.Reverse().ToArray();

        for (int i = 0; i < expected.Length; i++)
        {
            var msg = messages[i];
            Assert.Equal(expected[i], msg.Item1);
            Assert.Equal(nameof(T08_PublishToTopicAndReceive), msg.Item2);
        }

        factory.Model.ExchangeDelete(exchange.Name, false);
    }
}

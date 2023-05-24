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
    public void T01_PublishToFanout()
    {
        using var factory = new ModelFactory();
        var info = ChannelInfo.FanoutExchange("Test_PublishToFanout");

        using var publisher = new Publisher(
            factory.ChannelManager,
            Channels.ChannelStrategy.Pooled,
            info,
            factory.CreateLogger<Publisher>());

        var message = new PublicationMessage(nameof(T01_PublishToFanout));

        publisher.Publish(message);

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
                factory.ChannelManager,
                Channels.ChannelStrategy.Shared,
                info,
                factory.CreateLogger<Publisher>());
        });
        
        factory.Model.ExchangeDelete(info.Name, false);
    }

    [Fact]
    public void T03_PublishToQueue()
    {
        using var factory = new ModelFactory();
        var info = ChannelInfo.TemporaryQueue();

        using var publisher = new Publisher(
            factory.ChannelManager, 
            Channels.ChannelStrategy.Pooled,
            info,
            factory.CreateLogger<Publisher>());

        var message = new PublicationMessage(nameof(T03_PublishToQueue));

        publisher.Publish(message);
    }

    [Fact]
    public async Task T04_PublishToQueueAndReceive()
    {
        using var factory = new ModelFactory();
        var info = ChannelInfo.TemporaryQueue();

        string? msg = null;
        void Consumer(object? sender, BasicDeliverEventArgs e) 
        {
            msg = Encoding.UTF8.GetString(e.Body.Span);
        }

        var listener = new MessageListener(Consumer);
        using var receiver = new Receiver(
            factory.ChannelManager,
            Channels.ChannelStrategy.Shared,
            info,
            factory.CreateLogger<Receiver>());

        receiver.Listen(listener);

        using var publisher = new Publisher(
            factory.ChannelManager, 
            Channels.ChannelStrategy.Pooled,
            info,
            factory.CreateLogger<Publisher>());

        var message = new PublicationMessage(nameof(T04_PublishToQueueAndReceive));

        publisher.Publish(message);

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
        }

        var listener = new MessageListener(Consumer);
        using var receiver = new Receiver(
            factory.ChannelManager,
            Channels.ChannelStrategy.Shared,
            queue,
            factory.CreateLogger<Receiver>());

        receiver.Listen(listener);

        using var publisher = new Publisher(
            factory.ChannelManager,
            Channels.ChannelStrategy.Pooled, 
            exchange,
            factory.CreateLogger<Publisher>());

        var message = new PublicationMessage(nameof(T05_PublishToFanoutAndReceive));

        publisher.Publish(message);

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
        }

        var listener = new MessageListener(Consumer);
        using var receiver = new Receiver(
            factory.ChannelManager,
            Channels.ChannelStrategy.Shared,
            queue1,
            factory.CreateLogger<Receiver>());

        receiver.Listen(listener);

        using var publisher = new Publisher(
            factory.ChannelManager, 
            Channels.ChannelStrategy.Pooled,
            exchange,
            factory.CreateLogger<Publisher>());

        var message = new PublicationMessage(nameof(T06_PublishToRouteAndReceive1))
        {
            RoutingKey = "orange"
        };
        publisher.Publish(message);

        message = new PublicationMessage(nameof(T06_PublishToRouteAndReceive1))
        {
            RoutingKey = "black"
        };
        publisher.Publish(message);

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
        }

        var listener = new MessageListener(Consumer);

        using var receiver1 = new Receiver(
            factory.ChannelManager, 
            Channels.ChannelStrategy.Shared,
            queue1,
            factory.CreateLogger<Receiver>());
        receiver1.Listen(listener);

        using var receiver2 = new Receiver(
            factory.ChannelManager,
            Channels.ChannelStrategy.Shared,
            queue2,
            factory.CreateLogger<Receiver>());
        receiver2.Listen(listener);

        using var publisher = new Publisher(
            factory.ChannelManager,
            Channels.ChannelStrategy.Pooled, 
            exchange, 
            factory.CreateLogger<Publisher>());

        var message = new PublicationMessage(nameof(T07_PublishToRouteAndReceive2))
        {
            RoutingKey = "orange"
        };
        publisher.Publish(message);

        message = new PublicationMessage(nameof(T07_PublishToRouteAndReceive2))
        {
            RoutingKey = "black"
        };
        publisher.Publish(message);

        message = new PublicationMessage(nameof(T07_PublishToRouteAndReceive2))
        {
            RoutingKey = "green"
        };
        publisher.Publish(message);

        message = new PublicationMessage(nameof(T07_PublishToRouteAndReceive2))
        {
            RoutingKey = "white"
        };
        publisher.Publish(message);

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
        }

        var listener = new MessageListener(Consumer);

        using var receiver1 = new Receiver(
            factory.ChannelManager,
            Channels.ChannelStrategy.Shared,
            queue1, 
            factory.CreateLogger<Receiver>());
        receiver1.Listen(listener);

        using var receiver2 = new Receiver(
            factory.ChannelManager, 
            Channels.ChannelStrategy.Shared,
            queue2,
            factory.CreateLogger<Receiver>());
        receiver2.Listen(listener);

        using var publisher = new Publisher(
            factory.ChannelManager, 
            Channels.ChannelStrategy.Pooled,
            exchange, 
            factory.CreateLogger<Publisher>());

        var message = new PublicationMessage(nameof(T08_PublishToTopicAndReceive))
        {
            RoutingKey = "quick.orange.rabbit"
        };
        publisher.Publish(message);
        await Task.Delay(10);

        message = new PublicationMessage(nameof(T08_PublishToTopicAndReceive))
        {
            RoutingKey = "lazy.orange.elephant"
        };
        publisher.Publish(message);
        await Task.Delay(10);

        message = new PublicationMessage(nameof(T08_PublishToTopicAndReceive))
        {
            RoutingKey = "quick.orange.fox"
        };
        publisher.Publish(message);
        await Task.Delay(10);

        message = new PublicationMessage(nameof(T08_PublishToTopicAndReceive))
        {
            RoutingKey = "lazy.brown.fox"
        };
        publisher.Publish(message);
        await Task.Delay(10);

        message = new PublicationMessage(nameof(T08_PublishToTopicAndReceive))
        {
            RoutingKey = "lazy.pink.rabbit"
        };
        publisher.Publish(message);
        await Task.Delay(10);

        message = new PublicationMessage(nameof(T08_PublishToTopicAndReceive))
        {
            RoutingKey = "quick.brown.fox"
        };
        publisher.Publish(message);
        await Task.Delay(10);

        message = new PublicationMessage(nameof(T08_PublishToTopicAndReceive))
        {
            RoutingKey = "quick.orange.male.rabbit"
        };
        publisher.Publish(message);
        await Task.Delay(10);

        message = new PublicationMessage(nameof(T08_PublishToTopicAndReceive))
        {
            RoutingKey = "lazy.orange.male.rabbit"
        };
        publisher.Publish(message);

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

using System.Collections.Generic;

namespace RoyalCode.RabbitMQ.Components.Communication;

/// <summary>
/// <para>
///     Information for declaring the Dead Letter functionality for a queue.
/// </para>
/// </summary>
public class DeadLetterInfo
{
    /// <summary>
    /// Determines whether the queue will have declared Dead Letters functionality.
    /// </summary>
    public bool Active { get; set; } = false;

    /// <summary>
    /// Exchange used for Dead Letter.
    /// </summary>
    public string Exchange { get; set; } = Constants.DefaultDeadLetterExchange;

    /// <summary>
    /// Route type, default <see cref="DeadLetterRoutingKind.UseQueueName"/>.
    /// </summary>
    public DeadLetterRoutingKind RoutingKind { get; set; }

    /// <summary>
    /// Route to the exchange.
    /// </summary>
    public string? RoutingKey { get; set; }

    /// <summary>
    /// Includes the arguments of the DeadLetter functionality, if it is active.
    /// </summary>
    /// <param name="queueArguments">Arguments for queue.</param>
    /// <param name="queueName">Queue Name, to be used if the type of route key is the queue name.</param>
    public void Include(Dictionary<string, object> queueArguments, string? queueName)
    {
        if (Active)
        {
            if (string.IsNullOrWhiteSpace(Exchange))
                throw new ChannelConfiguraException("The Exchange for dead letter is invalid.");

            queueArguments["x-dead-letter-exchange"] = Exchange;

            switch (RoutingKind)
            {
                case DeadLetterRoutingKind.UseQueueName:

                    if (string.IsNullOrWhiteSpace(queueName))
                        throw new ChannelConfiguraException("Invalid queue name for declare as dead letter routing key.");

                    queueArguments["x-dead-letter-routing-key"] = queueName;
                    break;

                case DeadLetterRoutingKind.UseSpecifiedValue:

                    if (string.IsNullOrWhiteSpace(RoutingKey))
                        throw new ChannelConfiguraException("The specified routing key for dead letter is invalid.");

                    queueArguments["x-dead-letter-routing-key"] = RoutingKey;
                    break;

                case DeadLetterRoutingKind.None:
                    break;
            }

        }
    }

    /// <summary>
    /// Creates arguments to queue including DeadLetter functionality, if active.
    /// </summary>
    /// <param name="queueName">Queue Name, to be used if the type of route key is the queue name.</param>
    /// <returns>The arguments for the queue.</returns>
    public Dictionary<string, object> CreateArguments(string queueName)
    {
        var arguments = new Dictionary<string, object>();
        Include(arguments, queueName);
        return arguments;
    }

    /// <summary>
    /// It populates the properties, used in the creation of a channel, with deadletter information.
    /// </summary>
    /// <param name="dic">Channel properties dictionary.</param>
    /// <param name="name">Name of the queue.</param>
    internal void Properties(Dictionary<string, string> dic, string name)
    {
        var arguments = CreateArguments(name);
        foreach (var arg in arguments)
        {
            dic.Add(arg.Key, (string)arg.Value);
        }
    }

    /// <summary>
    /// <para>
    ///     Constants of dead letters.
    /// </para>
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Default exchage for Dead Letters.
        /// </summary>
        public const string DefaultDeadLetterExchange = "DeadLetters";
    }
}

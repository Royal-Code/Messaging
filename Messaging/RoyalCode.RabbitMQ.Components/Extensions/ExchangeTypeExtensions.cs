using System;
using RoyalCode.RabbitMQ.Components.Declarations;

namespace RoyalCode.RabbitMQ.Components.Communication;

/// <summary>
/// Extensions methods for <see cref="ExchangeType"/>.
/// </summary>
public static class ExchangeTypeExtensions
{
    /// <summary>
    /// Gets the string representing the RabbitMQ exchange type used for exchage declaration.
    /// </summary>
    /// <param name="type">Exchange type.</param>
    /// <returns>RabbitMQ exchange type.</returns>
    public static string ToExchageType(this ExchangeType type)
    {
        return type switch
        {
            ExchangeType.Route => ExchangeTypes.Direct,
            ExchangeType.Fanout => ExchangeTypes.Fanout,
            ExchangeType.Topic => ExchangeTypes.Topic,
            _ => throw new NotSupportedException("RabbitMQ channel type not supported as ExchangeType."),
        };
    }

    /// <summary>
    /// Constants for the RabbitMQ exchanges types.
    /// </summary>
    public static class ExchangeTypes
    {
        /// <summary>
        /// direct
        /// </summary>
        public const string Direct = "direct";

        /// <summary>
        /// fanout
        /// </summary>
        public const string Fanout = "fanout";

        /// <summary>
        /// topic
        /// </summary>
        public const string Topic = "topic";
    }
}
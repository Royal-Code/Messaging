using RabbitMQ.Client;
using System;

namespace RoyalCode.RabbitMQ.Components.Communication;

/// <summary>
/// <para>
///     Details of a message to be published to RabbitMQ.
/// </para>
/// </summary>
public class PublicationMessage
{
    /// <summary>
    /// Creates a new message.
    /// </summary>
    /// <param name="configureProperties">Action to configure basic properties of the publication, like headers.</param>
    /// <param name="body">The message content.</param>
    public PublicationMessage(Action<IBasicProperties> configureProperties, ReadOnlyMemory<byte> body)
    {
        ConfigureProperties = configureProperties;
        Body = body;
    }

    /// <summary>
    /// Action to configure basic properties of the publication, like headers.
    /// </summary>
    public Action<IBasicProperties>? ConfigureProperties { get; }

    /// <summary>
    /// The message content.
    /// </summary>
    public ReadOnlyMemory<byte> Body { get; }

    /// <summary>
    /// Optional rounting key for publication.
    /// </summary>
    public string? RoutingKey { get; set; }
}
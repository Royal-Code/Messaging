using RabbitMQ.Client;
using System;
using System.Text;

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
    /// <param name="body">The message content.</param>
    /// <param name="configureProperties">Action to configure basic properties of the publication, like headers.</param>
    public PublicationMessage(ReadOnlyMemory<byte> body, Action<IBasicProperties>? configureProperties = null)
    {
        ConfigureProperties = configureProperties;
        Body = body;
    }

    /// <summary>
    /// Creates a new message.
    /// </summary>
    /// <param name="body">The message content.</param>
    /// <param name="configureProperties">Action to configure basic properties of the publication, like headers.</param>
    public PublicationMessage(string body, Action<IBasicProperties>? configureProperties = null)
    {
        ConfigureProperties = configureProperties;
        Body = Encoding.UTF8.GetBytes(body);
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
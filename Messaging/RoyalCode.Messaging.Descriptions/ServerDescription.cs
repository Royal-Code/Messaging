namespace RoyalCode.Messaging.Descriptions;

/// <summary>
/// <para>
///     Description of a broker server.
/// </para>
/// </summary>
public class ServerDescription
{
    /// <summary>
    /// Creates a new server description.
    /// </summary>
    /// <param name="broker">See <see cref="Broker"/>.</param>
    /// <param name="name">See <see cref="Name"/>.</param>
    /// <param name="environment">See <see cref="Environment"/>.</param>
    /// <param name="address">See <see cref="Address"/>.</param>
    /// <param name="protocol">See <see cref="Protocol"/>.</param>
    /// <param name="version">See <see cref="Version"/>.</param>
    /// <param name="description">See <see cref="Description"/>.</param>
    public ServerDescription(
        string broker,
        string name,
        string environment,
        Uri address,
        string protocol,
        string version,
        string? description = null)
    {
        Broker = broker ?? throw new ArgumentNullException(nameof(broker));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Environment = environment ?? throw new ArgumentNullException(nameof(environment));
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Description = description;
    }

    /// <summary>
    /// The server broker name.
    /// </summary>
    public string Broker { get; }

    /// <summary>
    /// The server name.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// The environment of the server.
    /// </summary>
    public string Environment { get; }
    
    /// <summary>
    /// <para>
    ///     The Address of the server, a URI to the target host.
    /// </para>
    /// </summary>
    public Uri Address { get; }

    /// <summary>
    /// The server protocol.
    /// </summary>
    public string Protocol { get; }
    
    /// <summary>
    /// The protocol version.
    /// </summary>
    public string Version { get; }
    
    /// <summary>
    /// Server description, optional.
    /// </summary>
    public string? Description { get; }
}
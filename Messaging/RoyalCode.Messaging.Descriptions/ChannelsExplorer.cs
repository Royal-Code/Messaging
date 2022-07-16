namespace RoyalCode.Messaging.Descriptions;

/// <summary>
/// <para>
///     A collection of descriptors of the messaging channels used by the application.
/// </para>
/// </summary>
public class ChannelsExplorer : List<ChannelDescription>
{
    /// <summary>
    /// Represents a unique identifier for the application.
    /// </summary>
    public ApplicationId? Id { get; set; }

    /// <summary>
    /// Information about the application.
    /// </summary>
    public ApplicationInfo? Info { get; set; }

    /// <summary>
    /// A collection of server description.
    /// </summary>
    public ICollection<ServerDescription> Servers { get; } = new LinkedList<ServerDescription>();

    /// <summary>
    /// A collection of channels descriptions.
    /// </summary>
    public ICollection<ChannelDescription> Channels { get; } = new LinkedList<ChannelDescription>();
}
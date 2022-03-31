using System.Text.Json.Serialization;

namespace RoyalCode.Messaging.Descriptions;

/// <summary>
/// <para>
///     Application information.
/// </para>
/// </summary>
public class ApplicationInfo
{
    /// <summary>
    /// Creates a new information.
    /// </summary>
    /// <param name="name">The application name, like a title.</param>
    /// <param name="description">escription of the application.</param>
    /// <param name="version">The current version.</param>
    /// <param name="additionalProperties">Optional additional properties.</param>
    /// <exception cref="ArgumentNullException">
    ///     If any parameter is null.
    /// </exception>
    public ApplicationInfo(string name, string description, string version, 
        IEnumerable<KeyValuePair<string, object>>? additionalProperties = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        
        if (additionalProperties is not null)
            foreach (var kvp in additionalProperties)
            {
                AdditionalProperties.Add(kvp.Key, kvp.Value);
            }
    }

    /// <summary>
    /// The application name, like a title.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Description of the application.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// The current version.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Adicional information.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object> AdditionalProperties { get; } = new();
}
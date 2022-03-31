namespace RoyalCode.Messaging.Descriptions;

/// <summary>
/// <para>
///     A value object that represents a unique identifier for the application.
/// </para>
/// <para>
///     It can be a universal identifier, preferably a URI.
/// </para>
/// <para>
///     In cases where you want to generate the document for the AsyncAPI it should be a URI according to the RFC3986 specification.
/// </para>
/// </summary>
public class ApplicationId
{
    /// <summary>
    /// Creates a new application id from a string.
    /// </summary>
    /// <param name="value">The identifier value as string.</param>
    /// <exception cref="ArgumentException">
    ///     If value is null or white space.
    /// </exception>
    public ApplicationId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
        
        Value = value;

        IsUriFormat = Uri.TryCreate(value, UriKind.Absolute, out var uri);
        Uri = uri;
    }

    /// <summary>
    /// Creates a new application id from a URI.
    /// </summary>
    /// <param name="uri">The identifier value as URI.</param>
    /// <exception cref="ArgumentNullException">
    ///     If uri is null.
    /// </exception>
    public ApplicationId(Uri uri)
    {
        Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        Value = uri.AbsoluteUri;
    }
    
    /// <summary>
    /// The identifier value as string.
    /// </summary>
    public string Value { get; }
    
    /// <summary>
    /// If the value format is a URI.
    /// </summary>
    public bool IsUriFormat { get; }
    
    /// <summary>
    /// The identifier value as URI, if applicable.
    /// </summary>
    public Uri? Uri { get; }
}
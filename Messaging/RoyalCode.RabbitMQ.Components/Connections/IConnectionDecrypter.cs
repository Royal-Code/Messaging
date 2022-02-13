namespace RoyalCode.RabbitMQ.Components.Connections;

/// <summary>
/// Component for decrypt connection strings.
/// </summary>
public interface IConnectionDecrypter
{
    /// <summary>
    /// Decrypt the connection string.
    /// </summary>
    /// <param name="connectionName">The name of the connection.</param>
    /// <param name="encryptedConnectionString">The encrypted connection string value.</param>
    /// <param name="decryptedConnectionString">The decrypted connection string value.</param>
    /// <returns>True if the string was decripted, false otherwise</returns>
    /// <exception cref="System.NotSupportedException">
    ///     When the connection string is encrypted and cannot be decrypted for some reason.
    /// </exception>
    bool TryDecrypt(string connectionName, string encryptedConnectionString, out string decryptedConnectionString);
}
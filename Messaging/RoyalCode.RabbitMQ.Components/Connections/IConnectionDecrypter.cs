namespace RoyalCode.RabbitMQ.Components.Connections
{
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
        /// <returns>The decrypted connection string value.</returns>
        string Decrypt(string connectionName, string encryptedConnectionString);
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace RoyalCode.RabbitMQ.Components.Connections;

/// <summary>
/// <para>
///     This is a internal component and is not designed to be used by end user.
/// </para>
/// <para>
///     This factory creates <see cref="IConnectionPool"/> for a given name, 
///     using the <see cref="ConnectionPoolOptions"/> getted from <see cref="IOptionsMonitor{TOptions}"/>.
/// </para>
/// </summary>
public class ConnectionPoolFactory
{
    private readonly IOptionsMonitor<ConnectionPoolOptions> options;
    private readonly IConfiguration configuration;
    private readonly IConnectionDecrypter? decrypter;

    /// <summary>
    /// Creates a new factory.
    /// </summary>
    /// <param name="options">The options to build the pools.</param>
    /// <param name="configuration">The application configurations.</param>
    /// <param name="decrypter">Optional connection decrypter.</param>
    /// <exception cref="ArgumentNullException">
    ///     If one argument is null.
    /// </exception>
    public ConnectionPoolFactory(
        IOptionsMonitor<ConnectionPoolOptions> options,
        IConfiguration configuration,
        IConnectionDecrypter? decrypter = null)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.decrypter = decrypter;
    }

    /// <summary>
    /// Creates a new connection pool for a given name.
    /// </summary>
    /// <param name="name">The name of connection.</param>
    /// <returns>A new instance of connection pool.</returns>
    /// <exception cref="InvalidOperationException">
    ///     If none connection was configurated for the given name.
    /// </exception>
    public IConnectionPool Create(string name)
    {
        var options = this.options.Get(name);

        if (!options.HasConnections())
            throw new InvalidOperationException($"None connection configurated for name '{name}'");

        var connectionStrings = GetConnectionStrings(options.csNames);

        var connectionFactories = CreateConnectionInfos(name, connectionStrings);

        if (options.ConnectionCreationInterceptor is not null)
            connectionFactories.Each(options.ConnectionCreationInterceptor);


        return new ConnectionPool(options.ShouldTryBackToFirstConnection,
            options.RetryConnectionDelay,
            connectionFactories);
    }

    private string[] GetConnectionStrings(List<string> csNames)
    {
        string[] connectionStrings = new string[csNames.Count];

        for (int i = 0; i < connectionStrings.Length; i++)
        {
            var csName = csNames[i];

            var cs = configuration.GetConnectionString(csName);
            if (cs is null)
                throw new InvalidOperationException($"The connection string with name '{csName}' was not found");

            if (decrypter?.TryDecrypt(csName, cs, out var decrypted) ?? false)
                cs = decrypted;

            connectionStrings[i] = cs;
        }

        return connectionStrings;
    }

    private ConnectionFactory[] CreateConnectionInfos(string name, string[] connectionStrings)
    {
        var connections = new ConnectionFactory[connectionStrings.Length];

        for (int i = 0; i < connectionStrings.Length; i++)
        {
            var cs = connectionStrings[i];

            var parts = cs.Split(';');
            foreach (var part in parts)
            {
                if (part.Length == 0)
                    continue;

                var pos = part.IndexOf('=');
                if (pos != -1)
                    throw new InvalidOperationException($"Invalid connection string, the name is '{name}'.");

                var paramName = part[..pos];
                var value = part[(pos + 1)..];

                var cf = new ConnectionFactory();

                if (paramName.Equals(Parameters.HostName, StringComparison.OrdinalIgnoreCase))
                    cf.HostName = value;
                else if (paramName.Equals(Parameters.Port, StringComparison.OrdinalIgnoreCase))
                    cf.Port = int.Parse(value);
                else if (paramName.Equals(Parameters.UserName, StringComparison.OrdinalIgnoreCase))
                    cf.UserName = value;
                else if (paramName.Equals(Parameters.Password, StringComparison.OrdinalIgnoreCase))
                    cf.Password = value;
                else if (paramName.Equals(Parameters.VirtualHost, StringComparison.OrdinalIgnoreCase)
                        || paramName.Equals(Parameters.VHost, StringComparison.OrdinalIgnoreCase))
                    cf.VirtualHost = value;
                else if (paramName.Equals(Parameters.DispatchConsumersAsync))
                    cf.DispatchConsumersAsync = bool.Parse(value);
                else if (paramName.Equals(Parameters.Uri, StringComparison.OrdinalIgnoreCase))
                    cf.Uri = new Uri(value);
                else
                    throw new InvalidOperationException(
                        $"Connection string with name is '{name}' has a invalid property name ({paramName}).");

                connections[i] = cf;
            }
        }

        return connections;
    }

    /// <summary>
    /// <para>
    ///     Connection string properties names.
    /// </para>
    /// <para>
    ///     Used to set values to <see cref="ConnectionFactory"/>.
    /// </para>
    /// </summary>
    public static class Parameters
    {
        /// <summary>
        /// HostName
        /// </summary>
        public const string HostName = "HostName";

        /// <summary>
        /// UserName
        /// </summary>
        public const string UserName = "UserName";

        /// <summary>
        /// Password
        /// </summary>
        public const string Password = "Password";

        /// <summary>
        /// Port
        /// </summary>
        public const string Port = "Port";

        /// <summary>
        /// VHost
        /// </summary>
        public const string VHost = "VHost";

        /// <summary>
        /// VirtualHost
        /// </summary>
        public const string VirtualHost = "VirtualHost";

        /// <summary>
        /// DispatchConsumersAsync
        /// </summary>
        public const string DispatchConsumersAsync = "DispatchConsumersAsync";

        /// <summary>
        /// Uri
        /// </summary>
        public const string Uri = "Uri";
    }
}
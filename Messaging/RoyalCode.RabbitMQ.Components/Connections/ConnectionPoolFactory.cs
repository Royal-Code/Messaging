﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

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
public sealed class ConnectionPoolFactory
{
    private readonly IOptionsMonitor<ConnectionPoolOptions> options;
    private readonly IConfiguration configuration;
    private readonly ILoggerFactory loggerFactory;
    private readonly IConnectionDecrypter? decrypter;

    /// <summary>
    /// Creates a new factory.
    /// </summary>
    /// <param name="options">The connectionOptions to build the connections.</param>
    /// <param name="configuration">The application configurations.</param>
    /// <param name="loggerFactory">Logger factory.</param>
    /// <param name="decrypter">Optional connection decrypter.</param>
    /// <exception cref="ArgumentNullException">
    ///     If one argument is null.
    /// </exception>
    public ConnectionPoolFactory(
        IOptionsMonitor<ConnectionPoolOptions> options,
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        IConnectionDecrypter? decrypter = null)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.loggerFactory = loggerFactory;
        this.decrypter = decrypter;
    }

    /// <summary>
    /// Creates a new connection pool for a given name.
    /// </summary>
    /// <param name="name">The name of RabbitMQ Cluster.</param>
    /// <returns>A new instance of connection pool.</returns>
    /// <exception cref="InvalidOperationException">
    ///     If none connection was configurated for the given name.
    /// </exception>
    public IConnectionPool Create(string name)
    {
        var connectionOptions = options.Get(name);

        if (!connectionOptions.HasConnections())
            throw new InvalidOperationException($"None connection configurated for RabbitMQ Cluster name '{name}'");

        var connectionStrings = GetConnectionStrings(connectionOptions.ConnectionStringNames);

        var connectionFactories = CreateConnectionInfos(name, connectionStrings);

        if (connectionOptions.ConnectionCreationInterceptor is not null)
            connectionFactories.Each(connectionOptions.ConnectionCreationInterceptor);


        return new ConnectionPool(connectionOptions.ShouldTryBackToFirstConnection,
            connectionOptions.RetryConnectionDelay,
            connectionFactories,
            loggerFactory.CreateLogger<ConnectionPool>());
    }

    private string[] GetConnectionStrings(IList<string> csNames)
    {
        string[] connectionStrings = new string[csNames.Count];

        for (int i = 0; i < connectionStrings.Length; i++)
        {
            var csName = csNames[i];

            var cs = configuration.GetConnectionString(csName)
                ?? throw new InvalidOperationException($"The connection string with name '{csName}' was not found");

            if (decrypter?.TryDecrypt(csName, cs, out var decrypted) ?? false)
                cs = decrypted;

            connectionStrings[i] = cs;
        }

        return connectionStrings;
    }

    private static ConnectionFactory[] CreateConnectionInfos(string name, string[] connectionStrings)
    {
        var connections = new ConnectionFactory[connectionStrings.Length];

        for (int i = 0; i < connectionStrings.Length; i++)
        {
            var cs = connectionStrings[i];

            var cf = new ConnectionFactory
            {
                AutomaticRecoveryEnabled = true,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(120)
            };

            var parts = cs.Split(';');
            foreach (var part in parts)
            {
                if (part.Length == 0)
                    continue;

                var pos = part.IndexOf('=');
                if (pos is -1)
                    throw new InvalidOperationException($"Invalid connection string, the name is '{name}'.");

                var paramName = part[..pos];
                var value = part[(pos + 1)..];

                if (paramName.Equals(Parameters.HostName, StringComparison.OrdinalIgnoreCase)
                    || paramName.Equals(Parameters.Host, StringComparison.OrdinalIgnoreCase))
                    cf.HostName = value;
                else if (paramName.Equals(Parameters.Port, StringComparison.OrdinalIgnoreCase))
                    cf.Port = int.Parse(value);
                else if (paramName.Equals(Parameters.UserName, StringComparison.OrdinalIgnoreCase)
                    || paramName.Equals(Parameters.User, StringComparison.OrdinalIgnoreCase))
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
            }

            connections[i] = cf;
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
        /// Host
        /// </summary>
        public const string Host = "Host";

        /// <summary>
        /// UserName
        /// </summary>
        public const string UserName = "UserName";

        /// <summary>
        /// User
        /// </summary>
        public const string User = "User";

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
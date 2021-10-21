﻿using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace RoyalCode.RabbitMQ.Components.Connections
{
    public class ConnectionPoolOptions
    {
        internal List<string> csNames = new List<string>();

        /// <summary>
        /// If connection manager should try to go back to the first connection.
        /// </summary>
        public bool ShouldTryBackToFirstConnection { get; set; } = true;

        /// <summary>
        /// Waiting time to try to connect again.
        /// </summary>
        public TimeSpan RetryConnectionDelay { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// The callback for extra configuration for <see cref="ConnectionFactory"/>.
        /// </summary>
        public Action<ConnectionFactory>? CreationCallback { get; set; }

        /// <summary>
        /// <para>
        ///     Add a connection string name.
        /// </para>
        /// <para>
        ///     The connection string will be readed from the configuration.
        /// </para>
        /// </summary>
        /// <param name="name">The name of the connection string.</param>
        /// <returns>
        ///     The same instance.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If <paramref name="name"/> is null or white space.
        /// </exception>
        public ConnectionPoolOptions AddConnectionStringName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));

            csNames.Add(name);
            return this;
        }

        /// <summary>
        /// Check if an connection was added.
        /// </summary>
        /// <returns>True if one or more connections was added.</returns>
        public bool HasConnections() => csNames.Count > 0;
    }
}

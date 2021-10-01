using System;
using System.Collections.Generic;

namespace RoyalCode.Messaging
{
    /// <summary>
    /// Uma mensagem para ser publicada pelo broker, gerada através de um <see cref="IPublisher{TMessage}"/>.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IOutgoingMessage<TMessage> : IOutgoingMessage { }

    /// <summary>
    /// Uma mensagem para ser publicada pelo broker, gerada através de um <see cref="IPublisher{TMessage}"/>.
    /// </summary>
    public interface IOutgoingMessage
    {
        /// <summary>
        /// <para>
        ///     Id da mensagem.
        /// </para>
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Tipo (classe) do objeto da mensagem.
        /// </summary>
        Type MessageType { get; }

        /// <summary>
        /// Tipo (formato) do conteúdo (body).
        /// </summary>
        /// <example>
        /// application/json
        /// </example>
        string ContentType { get; }

        /// <summary>
        /// Enconding do conteúdo (body).
        /// </summary>
        /// <example>
        /// utf8
        /// </example>
        string ContentEncoding { get; }

        /// <summary>
        /// Conteúdo da mensagem.
        /// </summary>
        byte[] Body { get; }

        /// <summary>
        /// Propriedades da mensagem.
        /// </summary>
        IEnumerable<KeyValuePair<string, object>> Properties { get; }

        /// <summary>
        /// <para>
        ///     Usuário emissor da mensagem (código, subject, identificador).
        /// </para>
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// O nome do broker.
        /// </summary>
        string Broker { get; }
    }
}

using System;
using System.Collections.Generic;

namespace RoyalCode.Core.Messaging
{
    /// <summary>
    /// Mensagem recebida pelo cliente de um broker, a qual poderá ser escutada por um <see cref="IReceiver{TMessage}"/>;
    /// </summary>
    /// <typeparam name="TMessage">Tipo da mensagem recebida.</typeparam>
    public interface IIncomingMessage<TMessage> : IIncomingMessage
    {
        /// <summary>
        /// A mensagem.
        /// </summary>
        new TMessage Payload { get; }
    }

    /// <summary>
    /// Mensagem recebida pelo cliente de um broker, a qual poderá ser escutada por um <see cref="IReceiver{TMessage}"/>;
    /// </summary>
    public interface IIncomingMessage
    {
        /// <summary>
        /// <para>
        ///     Id da mensagem.
        /// </para>
        /// <para>
        ///     Se tipo de <see cref="Payload"/> implementar <see cref="IHasId{TId}"/> ou <see cref="IHasGuid"/>
        ///     este Id deverá ser o mesmo da mensagem (<see cref="Payload"/>).
        /// </para>
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Tipo (classe) do objeto da mensagem.
        /// </summary>
        Type MessageType { get; }

        /// <summary>
        /// Coleção de propriedades enviadas/publicadas adjunto a mensagem.
        /// </summary>
        IEnumerable<KeyValuePair<string, object>> Properties { get; }

        /// <summary>
        /// <para>
        ///     Usuário remetente da mensagem (código, subject, identificador).
        /// </para>
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// O nome do broker.
        /// </summary>
        string Broker { get; }

        /// <summary>
        /// A mensagem.
        /// </summary>
        object Payload { get; }

        /// <summary>
        /// <para>
        ///     Marca a mensagem para ser rejeitada.
        /// </para>
        /// <para>
        ///     A mensagem será descartada, podendo cair um deadletter se configurado.
        /// </para>
        /// </summary>
        void Reject();

        /// <summary>
        /// <para>
        ///     Marca a mensagem como rejeitada porém para ser reentregue.
        /// </para>
        /// <para>
        ///     Por padrão o tempo de sono é de 2000 milisegundos. Esse valor pode ser alterado através do
        ///     parâmetro <paramref name="sleepTime"/>.
        /// </para>
        /// <para>
        ///     O tempo de sono é importante para a reentrega, pois o sistema pode ficar em loop pedindo 
        ///     reentregas e isso pode acabar consumindo recursos desnecessários.
        /// </para>
        /// </summary>
        /// <param name="sleepTime">Tempo de sono em milisegundos.</param>
        void Redelivery(int sleepTime = 2000);
    }
}

using System.Threading.Tasks;

namespace RoyalCode.Core.Messaging
{
    /// <summary>
    /// <para>
    ///     Componente para enviar e receber mensagens entre sistemas.
    /// </para>
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Obtém um publicador de mensagens de um determinado tipo.
        /// </summary>
        /// <typeparam name="TMessage">Tipo da mensagem.</typeparam>
        /// <returns>Uma instância de um publicador de mensagens.</returns>
        IPublisher<TMessage> CreatePublisher<TMessage>();

        /// <summary>
        /// Obtém um serviço para escuta de filas de mensagens de um determinado tipo.
        /// </summary>
        /// <typeparam name="TMessage">Tipo da mensagem.</typeparam>
        /// <returns>Uma instância da escuta de mensagens.</returns>
        IReceiver<TMessage> CreateReceiver<TMessage>();

        /// <summary>
        /// Envia uma mensagem com os informações do modelo de dados.
        /// </summary>
        /// <param name="instance">Mensagem para enviar.</param>
        Task Publish<TMessage>(TMessage instance);
    }
}

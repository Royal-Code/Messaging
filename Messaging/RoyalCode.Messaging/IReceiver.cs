using System;
using System.Threading.Tasks;

namespace RoyalCode.Messaging
{
    /// <summary>
    /// Serviço que escuta filas de mensageria e repassa a mensagem para ouvintes.
    /// </summary>
    /// <typeparam name="TMessage">Modelo de dados obtido via mensageria.</typeparam>
    public interface IReceiver<TMessage> : IDisposable
    {
        /// <summary>
        /// Inicia a escuta de um canal enviando as mensagens para o ouvinte.
        /// </summary>
        /// <param name="handler">Componente que processa as mensagens recebidas do canal.</param>
        void Listen(Func<TMessage, Task> handler);

        /// <summary>
        /// Inicia a escuta de um canal enviando as mensagens para o ouvinte.
        /// </summary>
        /// <param name="handler">Componente que processa as mensagens recebidas do canal.</param>
        void Listen(Func<IIncomingMessage<TMessage>, Task> handler);
    }
}
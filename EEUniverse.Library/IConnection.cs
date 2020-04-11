using System;
using System.Threading.Tasks;

namespace EEUniverse.Library
{
    /// <summary>
    /// Provides a connection for interacting with the Everybody Edits Universe™ servers
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// An event that raises when the client receives a message with the assigned scope.
        /// </summary>
        event EventHandler<Message> OnMessage;

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="type">The type of the message.</param>
        /// <param name="data">An array of data to be sent.</param>
        void Send(MessageType type, params object[] data) => SendAsync(type, data).GetAwaiter().GetResult();

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        void Send(Message message) => SendAsync(message).GetAwaiter().GetResult();

        /// <summary>
        /// Sends an asynchronous message to the server.
        /// </summary>
        /// <param name="type">The type of the message.</param>
        /// <param name="data">An array of data to be sent.</param>
        Task SendAsync(MessageType type, params object[] data);

        /// <summary>
        /// Sends an asynchronous message to the server.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        Task SendAsync(Message message);
    }
}
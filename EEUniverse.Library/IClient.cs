using System;
using System.Threading.Tasks;

namespace EEUniverse.Library
{
	/// <summary>
	/// Provides a client for connecting to Everybody Edits Universe™
	/// </summary>
	public interface IClient : IAsyncDisposable
    {
        /// <summary>
        /// An event that raises when the client receives a message.
        /// </summary>
        event EventHandler<Message> OnMessage;

        /// <summary>
        /// An event that raises when the connection to the server is lost.
        /// </summary>
        event EventHandler<CloseEventArgs> OnDisconnect;

        /// <summary>
        /// The server to connect to.
        /// </summary>
        string MultiplayerHost { get; }

        /// <summary>
        /// Establishes a connection with the server and starts listening for messages.
        /// </summary>
        void Connect() => ConnectAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Establishes a connection with the server and starts listening for messages.
        /// </summary>
        Task ConnectAsync();

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="scope">The scope of the message.</param>
        /// <param name="type">The type of the message.</param>
        /// <param name="data">An array of data to be sent.</param>
        void Send(ConnectionScope scope, MessageType type, params object[] data) => Send(new Message(scope, type, data));

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        void Send(Message message) => SendAsync(message).GetAwaiter().GetResult();

        /// <summary>
        /// Sends a message to the server as an asynchronous operation.
        /// </summary>
        /// <param name="scope">The scope of the message.</param>
        /// <param name="type">The type of the message.</param>
        /// <param name="data">An array of data to be sent.</param>
        Task SendAsync(ConnectionScope scope, MessageType type, params object[] data) => SendAsync(new Message(scope, type, data));

        /// <summary>
        /// Sends a message to the server as an asynchronous operation.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public Task SendAsync(Message message);

        /// <summary>
        /// Creates a connection with the lobby.
        /// </summary>
        public IConnection CreateLobbyConnection();

        /// <summary>
        /// Creates a connection with the specified world.
        /// </summary>
        /// <param name="worldId">The world id to connect to.</param>
        public IConnection CreateWorldConnection(string worldId);
    }
}

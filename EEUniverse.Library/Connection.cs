using System;
using System.Threading.Tasks;

namespace EEUniverse.Library
{
    public class Connection
    {
        /// <summary>
        /// An event that raises when the client receives a message with the assigned scope.
        /// </summary>
        public event EventHandler<Message> OnMessage;

        private readonly Client _client;
        private readonly ConnectionScope _scope;

        /// <summary>
        /// Creates a new connection handler for the specified scope.
        /// </summary>
        /// <param name="client">The underlying client of this connection.</param>
        /// <param name="scope">The scope of the connection.<br />This is what the connection listens to in the OnMessage EventHandler.</param>
        /// <param name="worldId">The world ID to connect to.<br />This should only be filled when creating a connection with the 'World' scope.</param>
        public Connection(Client client, ConnectionScope scope, string worldId = "")
        {
            _client = client;
            _scope = scope;

            client.OnMessage += (s, message) => {
                if (message.Scope != scope)
                    return;

                OnMessage?.Invoke(this, message);
            };

            if (scope == ConnectionScope.World) {
                if (string.IsNullOrEmpty(worldId))
                    throw new ArgumentNullException($"{nameof(worldId)} should not be null or empty when the scope is world.");

                Send(new Message(ConnectionScope.Lobby, 0, "world", worldId));
            }
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="type">The type of the message.</param>
        /// <param name="data">An array of data to be sent.</param>
        public void Send(MessageType type, params object[] data) => _ = SendAsync(new Message(_scope, type, data));

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        public void Send(Message message) => _ = SendRawAsync(Serializer.Serialize(message));

        /// <summary>
        /// Sends an asynchronous message to the server.<br />Use with caution.
        /// </summary>
        /// <param name="buffer">The buffer containing the message to be sent.</param>
        public void SendRaw(ArraySegment<byte> buffer) => _ = SendRawAsync(buffer);

        /// <summary>
        /// Sends an asynchronous message to the server.<br />Use with caution.
        /// </summary>
        /// <param name="buffer">The buffer containing the message to be sent.</param>
        public void SendRaw(ReadOnlyMemory<byte> buffer) => SendRawAsync(buffer).GetAwaiter().GetResult();

        /// <summary>
        /// Sends an asynchronous message to the server.
        /// </summary>
        /// <param name="type">The type of the message.</param>
        /// <param name="data">An array of data to be sent.</param>
        public async Task SendAsync(MessageType type, params object[] data) => await SendRawAsync(Serializer.Serialize(new Message(_scope, type, data)));

        /// <summary>
        /// Sends an asynchronous message to the server.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        public async Task SendAsync(Message message) => await SendRawAsync(Serializer.Serialize(message));

        /// <summary>
        /// Sends an asynchronous message to the server.<br />Use with caution.
        /// </summary>
        /// <param name="buffer">The buffer containing the message to be sent.</param>
        public async Task SendRawAsync(ArraySegment<byte> buffer) => await _client.SendRawAsync(buffer);

        /// <summary>
        /// Sends an asynchronous message to the server.<br />Use with caution.
        /// </summary>
        /// <param name="bytes">The buffer containing the message to be sent.</param>
        public ValueTask SendRawAsync(ReadOnlyMemory<byte> bytes) => _client.SendRawAsync(bytes);
    }
}
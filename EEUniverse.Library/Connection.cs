using System;
using System.Threading.Tasks;

namespace EEUniverse.Library
{
    public class Connection : IConnection
    {
        /// <summary>
        /// An event that raises when the client receives a message with the assigned scope.
        /// </summary>
        public event EventHandler<Message> OnMessage;

        private readonly IClient _client;
        private readonly ConnectionScope _scope;

        /// <summary>
        /// Creates a new connection handler for the specified scope.
        /// </summary>
        /// <param name="client">The underlying client of this connection.</param>
        /// <param name="scope">The scope of the connection.<br />This is what the connection listens to in the OnMessage EventHandler.</param>
        /// <param name="worldId">The world ID to connect to.<br />This should only be filled when creating a connection with the 'World' scope.</param>
        public Connection(IClient client, ConnectionScope scope, string worldId = "")
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

                ((IConnection)this).Send(new Message(ConnectionScope.Lobby, 0, "world", worldId));
            }
        }

        public Task SendAsync(MessageType type, params object[] data) => _client.SendAsync(_scope, type, data);

        public Task SendAsync(Message message) => _client.SendAsync(message);
    }
}
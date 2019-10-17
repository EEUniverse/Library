﻿using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace EEUniverse.Library
{
    /// <summary>
    /// Provides a client for connecting to Everybody Edits Universe™
    /// </summary>
    public class Client
    {
        /// <summary>
        /// An event that raises when the client receives a message.
        /// </summary>
        public event EventHandler<Message> OnMessage;

        /// <summary>
        /// An event that raises when the connection to the server is lost.
        /// </summary>
        public event EventHandler<CloseEventArgs> OnDisconnect;

        /// <summary>
        /// The server to connect to.
        /// </summary>
        public string MultiplayerHost { get; private set; } = "wss://game.ee-universe.com";

        /// <summary>
        /// The maximum amount of data the client should try to read before deserializing the message.
        /// </summary>
        public int MaxBuffer { get; set; } = 1024 * 50; // 51.2 kb

        private Thread _messageReceiverThread;
        private readonly ClientWebSocket _socket;
        private readonly string _token;

        /// <summary>
        /// Initializes a new client.
        /// </summary>
        /// <param name="token">The JWT to connect with.</param>
        public Client(string token)
        {
            _socket = new ClientWebSocket();
            _token = token;
        }

        /// <summary>
        /// Establishes a connection with the server and starts listening for messages.
        /// </summary>
        public void Connect() => ConnectAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Establishes a connection with the server and starts listening for messages.
        /// </summary>
        public async Task ConnectAsync()
        {
            await _socket.ConnectAsync(new Uri($"{MultiplayerHost}/?a={_token}"), CancellationToken.None);

            _messageReceiverThread = new Thread(async () => await MessageReceiver());
            _messageReceiverThread.Start();
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="scope">The scope of the message.</param>
        /// <param name="type">The type of the message.</param>
        /// <param name="data">An array of data to be sent.</param>
        public void Send(ConnectionScope scope, MessageType type, params object[] data)
            => SendAsync(new Message(scope, type, data)).GetAwaiter().GetResult;

        /// <summary>
        /// Sends a message to the server as an asynchronous operation.
        /// </summary>
        /// <param name="scope">The scope of the message.</param>
        /// <param name="type">The type of the message.</param>
        /// <param name="data">An array of data to be sent.</param>
        public Task SendAsync(ConnectionScope scope, MessageType type, params object[] data) => SendAsync(new Message(scope, type, data));

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void Send(Message message)
            => SendAsync(message).GetAwaiter().GetResult;

        /// <summary>
        /// Sends a message to the server as an asynchronous operation.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public Task SendAsync(Message message) => SendRawAsync(Serializer.Serialize(message));

        /// <summary>
        /// Sends a message to the server.<br />Use with caution.
        /// </summary>
        /// <param name="bytes">The buffer containing the message to be sent.</param>
        public void SendRaw(ArraySegment<byte> bytes) => SendRawAsync(bytes).GetAwaiter().GetResult();

        /// <summary>
        /// Sends a message to the server as an asynchronous operation.<br />Use with caution.
        /// </summary>
        /// <param name="bytes">The buffer containing the message to be sent.</param>
        public Task SendRawAsync(ArraySegment<byte> bytes) => _socket.SendAsync(bytes, WebSocketMessageType.Binary, true, CancellationToken.None);

        /// <summary>
        /// Creates a connection with the lobby.
        /// </summary>
        public Connection CreateLobbyConnection() => new Connection(this, ConnectionScope.Lobby);

        /// <summary>
        /// Creates a connection with the specified world.
        /// </summary>
        /// <param name="worldId">The world id to connect to.</param>
        public Connection CreateWorldConnection(string worldId) => new Connection(this, ConnectionScope.World, worldId);

        /// <summary>
        /// An asynchronous listener to receive incomming messages from the Everybody Edits Universe™ server.
        /// </summary>
        private async Task MessageReceiver() //TODO: improve buffer system..?
        {
            var buffer = new byte[MaxBuffer];
            var count = 0;

            while (_socket.State == WebSocketState.Open) {
                try {
                    var result = await _socket.ReceiveAsync(buffer, default);
                    if (result.MessageType == WebSocketMessageType.Close)
                        break;

                    count += result.Count;
                    if (!result.EndOfMessage)
                        continue;

                    var bytes = new Span<byte>(buffer)[..count].ToArray();
                    var message = Serializer.Deserialize(bytes);
                    OnMessage?.Invoke(this, message);
                    count = 0;
                }
                catch (WebSocketException ex) {
                    OnDisconnect?.Invoke(this, new CloseEventArgs {
                        WasClean = false,
                        WebSocketError = ex.WebSocketErrorCode,
                        Reason = ex.Message
                    });

                    return;
                }
            }

            OnDisconnect?.Invoke(this, new CloseEventArgs {
                WasClean = true,
                WebSocketError = WebSocketError.Success,
                Reason = "Disconnected gracefully"
            });
        }
    }
}

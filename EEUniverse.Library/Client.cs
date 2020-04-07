using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace EEUniverse.Library
{
    /// <summary>
    /// The default implementation for a client connecting to Everybody Edits Universe™
    /// </summary>
    public class Client : IClient
    {
        public event EventHandler<Message> OnMessage;
        public event EventHandler<CloseEventArgs> OnDisconnect;

        public string MultiplayerHost { get; private set; } = "wss://game.ee-universe.com";

        /// <summary>
        /// The maximum amount of data the internal MemoryStream buffer can be before it forcilby shrinks itself.
        /// </summary>
        public int MaxBuffer { get; set; } = 1024 * 50; // 51.2 kb

        /// <summary>
        /// The minimum amount of data the client should allocate before deserializing a message.
        /// </summary>
        public int MinBuffer { get; set; } = 4096; // 4 kb

        public ClientWebSocket Socket { get; }

        private Thread _messageReceiverThread;
        private readonly string _token;

        /// <summary>
        /// Initializes a new client.
        /// </summary>
        /// <param name="token">The JWT to connect with.</param>
        public Client(string token)
        {
            Socket = new ClientWebSocket();
            _token = token;
        }

        public async Task ConnectAsync()
        {
            await Socket.ConnectAsync(new Uri($"{MultiplayerHost}/?a={_token}"), CancellationToken.None);

            _messageReceiverThread = new Thread(async () => await MessageReceiver());
            _messageReceiverThread.Start();
        }

        public async ValueTask DisposeAsync()
        {
            // WebSocketException: The remote party closed the WebSocket connection without completing the close handshake.
            // ^ this is thrown when WebSocketCLoseStatus.NormalClosure is used.

            await Socket.CloseAsync(WebSocketCloseStatus.Empty, statusDescription: null, cancellationToken: default).ConfigureAwait(false);
            Socket.Dispose();
        }

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
        public Task SendRawAsync(ArraySegment<byte> bytes) => Socket.SendAsync(bytes, WebSocketMessageType.Binary, true, CancellationToken.None);

        public IConnection CreateLobbyConnection() => new Connection(this, ConnectionScope.Lobby);

        public IConnection CreateWorldConnection(string worldId) => new Connection(this, ConnectionScope.World, worldId);

        /// <summary>
        /// An asynchronous listener to receive incomming messages from the Everybody Edits Universe™ server.
        /// </summary>
        private async Task MessageReceiver()
        {
            var tempBuffer = new Memory<byte>(new byte[MinBuffer]);
            var memoryStream = new MemoryStream(MinBuffer);

            try {
                while (Socket.State == WebSocketState.Open) {
                    try {
                        ValueWebSocketReceiveResult result;

                        do {
                            result = await Socket.ReceiveAsync(tempBuffer, default).ConfigureAwait(false);
                            if (result.MessageType == WebSocketMessageType.Close)
                                goto GRACEFUL_DISCONNECT;

                            memoryStream.Write(tempBuffer.Span[..result.Count]);
                        } while (!result.EndOfMessage);

                        memoryStream.SetLength(memoryStream.Position); // we don't want to read previous data

                        memoryStream.Position = 0; // reset position for reading
                        var message = Serializer.Deserialize(memoryStream);
                        memoryStream.Position = 0; // reset position for reuse

                        if (memoryStream.Capacity > MaxBuffer) {
                            memoryStream.Dispose();
                            memoryStream = new MemoryStream(MinBuffer);
                        }

                        OnMessage?.Invoke(this, message);
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

            GRACEFUL_DISCONNECT:
                OnDisconnect?.Invoke(this, new CloseEventArgs {
                    WasClean = true,
                    WebSocketError = WebSocketError.Success,
                    Reason = "Disconnected gracefully"
                });
            }
            finally {
                memoryStream.Dispose();
            }
        }
    }
}

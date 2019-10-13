using System;
using System.Net.WebSockets;

namespace EEUniverse.Library
{
    /// <summary>
    /// Provides data for the disconnect event of a client.
    /// </summary>
    public class CloseEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a value indicating wether the disconnect was clean or not.
        /// </summary>
        public bool WasClean { get; internal set; }

        /// <summary>
        /// The error of the disconnect event.
        /// </summary>
        public WebSocketError WebSocketError { get; internal set; }

        /// <summary>
        /// The reason of the close event.
        /// </summary>
        public string Reason { get; internal set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EEUniverse.Library
{
    /// <summary>
    /// Represents the type of a message.
    /// </summary>
    public enum MessageType
    {
        [Scope(ConnectionScope.None)] SelfInfo = 23,

        [Scope(ConnectionScope.World)] Init = 0,
        [Scope(ConnectionScope.World)] Ping = 1,
        [Scope(ConnectionScope.World)] Pong = 2,
        [Scope(ConnectionScope.World)] Chat = 3,
        [Scope(ConnectionScope.World)] ChatOld = 4,
        [Scope(ConnectionScope.World)] PlaceBlock = 5,
        [Scope(ConnectionScope.World)] PlayerJoin = 6,
        [Scope(ConnectionScope.World)] PlayerExit = 7,
        [Scope(ConnectionScope.World)] PlayerMove = 8,
        [Scope(ConnectionScope.World)] PlayerSmiley = 9,
        [Scope(ConnectionScope.World)] PlayerGod = 10,
        [Scope(ConnectionScope.World)] CanEdit = 11,
        [Scope(ConnectionScope.World)] Meta = 12,
        [Scope(ConnectionScope.World)] ChatInfo = 13,
        [Scope(ConnectionScope.World)] PlayerAdd = 14,
        [Scope(ConnectionScope.World)] ZoneCreate = 15,
        [Scope(ConnectionScope.World)] ZoneDelete = 16,
        [Scope(ConnectionScope.World)] ZoneEdit = 17,
        [Scope(ConnectionScope.World)] ZoneEnter = 18,
        [Scope(ConnectionScope.World)] ZoneExit = 19,
        [Scope(ConnectionScope.World)] LimitedEdit = 20,
        [Scope(ConnectionScope.World)] ChatPMTo = 21,
        [Scope(ConnectionScope.World)] ChatPMFrom = 22,
        [Scope(ConnectionScope.World)] Clear = 24,
        [Scope(ConnectionScope.World)] CanGod = 25,
        [Scope(ConnectionScope.World)] BgColor = 26,
        [Scope(ConnectionScope.World)] Won = 27,
        [Scope(ConnectionScope.World)] Reset = 28,
        [Scope(ConnectionScope.World)] Notify = 29,
        [Scope(ConnectionScope.World)] Teleport = 30,
        [Scope(ConnectionScope.World)] Effect = 31,
        [Scope(ConnectionScope.World)] SwitchLocal = 32,
        [Scope(ConnectionScope.World)] SwitchGlobal = 33,
        [Scope(ConnectionScope.World)] CoinGold = 34,
        [Scope(ConnectionScope.World)] CoinBlue = 35,

        //TODO: Should probably find a better way to implement these.
        //      Also don't know how accurate the names are.
        [Scope(ConnectionScope.Lobby)] RoomConnect = 0,
        [Scope(ConnectionScope.Lobby)] RoomDisconnect = 1,
        [Scope(ConnectionScope.Lobby)] LoadRooms = 2,
        [Scope(ConnectionScope.Lobby)] LoadStats = 3,
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class ScopeAttribute : Attribute
    {
        public ConnectionScope Scope { get; }

        public ScopeAttribute(ConnectionScope scope) => Scope = scope;
    }

    public static class MessageTypeExtensions
    {
        private static ConnectionScope GetScope(FieldInfo field) => field.GetCustomAttribute<ScopeAttribute>().Scope;

        private static readonly Dictionary<(ConnectionScope scope, MessageType type), string> _names = typeof(MessageType)
            .GetFields()
            .Where(field => field.IsStatic)
            .ToDictionary(
                field => (GetScope(field), (MessageType)field.GetValue(null)),
                field => field.Name
            );

        /// <summary>
        /// Returns a string that represents the current message.
        /// </summary>
        /// <param name="messageType">The type of the message.</param>
        /// <param name="connectionScope">The scope of the message.</param>
        public static string ToString(this MessageType messageType, ConnectionScope connectionScope)
        {
            var key = (connectionScope, messageType);
            if (_names.ContainsKey(key))
                return _names[key];

            return ((int)messageType).ToString();
        }
    }
}

namespace EEUniverse.Library
{
    /// <summary>
    /// Represents the type of a message.
    /// </summary>
    public enum MessageType
    {
        Init = 0,
        Ping = 1,
        Pong = 2,
        Chat = 3,
        ChatOld = 4,
        PlaceBlock = 5,
        PlayerJoin = 6,
        PlayerExit = 7,
        PlayerMove = 8,
        PlayerSmiley = 9,
        PlayerGod = 10,
        CanEdit = 11,
        Meta = 12,
        ChatInfo = 13,
        PlayerAdd = 14,
        ZoneCreate = 15,
        ZoneDelete = 16,
        ZoneEdit = 17,
        ZoneEnter = 18,
        ZoneExit = 19,
        LimitedEdit = 20,
        ChatPMTo = 21,
        ChatPMFrom = 22,
        SelfInfo = 23,
        Save = 24,

        //TODO: Should probably find a better way to implement these.
        //      Also don't know how accurate the names are.
        RoomConnect = 0,
        RoomDisconnect = 1,
        LoadRooms = 2,
    }
}

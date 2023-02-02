using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Navigator;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Filter;

namespace Plus.Communication.Packets.Incoming.Navigator;

internal class CreateFlatEvent : IPacketEvent
{
    private readonly IWordFilterManager _wordFilterManager;
    private readonly IRoomManager _roomManager;
    private readonly INavigatorManager _navigatorManager;

    public CreateFlatEvent(IWordFilterManager wordFilterManager, IRoomManager roomManager, INavigatorManager navigatorManager)
    {
        _wordFilterManager = wordFilterManager;
        _roomManager = roomManager;
        _navigatorManager = navigatorManager;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var rooms = RoomDataLoader.GetRoomsDataByOwnerSortByName(session.GetHabbo().Id);
        if (rooms.Count >= 500)
        {
            session.Send(new CanCreateRoomComposer(true, 500));
            return Task.CompletedTask;
        }
        var name = _wordFilterManager.CheckMessage(packet.ReadString());
        var description = _wordFilterManager.CheckMessage(packet.ReadString());
        var modelName = packet.ReadString();
        var category = packet.ReadInt();
        var maxVisitors = packet.ReadInt(); //10 = min, 25 = max.
        var tradeSettings = packet.ReadInt(); //2 = All can trade, 1 = owner only, 0 = no trading.
        if (name.Length < 3)
            return Task.CompletedTask;
        if (name.Length > 25)
            return Task.CompletedTask;
        if (!_roomManager.TryGetModel(modelName, out var model))
            return Task.CompletedTask;
        if (!_navigatorManager.TryGetSearchResultList(category, out var searchResultList))
            category = 36;
        if (searchResultList.CategoryType != NavigatorCategoryType.Category || searchResultList.RequiredRank > session.GetHabbo().Rank)
            category = 36;
        if (maxVisitors < 10 || maxVisitors > 25)
            maxVisitors = 10;
        if (tradeSettings < 0 || tradeSettings > 2)
            tradeSettings = 0;
        var newRoom = _roomManager.CreateRoom(session, name, description, category, maxVisitors, tradeSettings, model);
        if (newRoom != null) session.Send(new FlatCreatedComposer(newRoom.Id, name));

        session.GetHabbo().Messenger.NotifyChangesToFriends();
        return Task.CompletedTask;
    }
}
using System.Threading.Tasks;
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

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var rooms = RoomFactory.GetRoomsDataByOwnerSortByName(session.GetHabbo().Id);
        if (rooms.Count >= 500)
        {
            session.SendPacket(new CanCreateRoomComposer(true, 500));
            return Task.CompletedTask;
        }
        var name = _wordFilterManager.CheckMessage(packet.PopString());
        var description = _wordFilterManager.CheckMessage(packet.PopString());
        var modelName = packet.PopString();
        var category = packet.PopInt();
        var maxVisitors = packet.PopInt(); //10 = min, 25 = max.
        var tradeSettings = packet.PopInt(); //2 = All can trade, 1 = owner only, 0 = no trading.
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
        if (newRoom != null) session.SendPacket(new FlatCreatedComposer(newRoom.Id, name));
        if (session.GetHabbo() != null && session.GetHabbo().GetMessenger() != null)
            session.GetHabbo().GetMessenger().OnStatusChanged(true);
        return Task.CompletedTask;
    }
}
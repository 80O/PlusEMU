using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine;

internal class ApplyDecorationEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IAchievementManager _achievementManager;
    private readonly IQuestManager _questManager;
    private readonly IDatabase _database;

    public ApplyDecorationEvent(IRoomManager roomManager, IAchievementManager achievementManager, IQuestManager questManager, IDatabase database)
    {
        _roomManager = roomManager;
        _achievementManager = achievementManager;
        _questManager = questManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        if (!room.CheckRights(session, true))
            return Task.CompletedTask;
        var item = session.GetHabbo().GetInventoryComponent().GetItem(packet.PopInt());
        if (item == null)
            return Task.CompletedTask;
        if (item.GetBaseItem() == null)
            return Task.CompletedTask;
        var decorationKey = string.Empty;
        switch (item.GetBaseItem().InteractionType)
        {
            case InteractionType.Floor:
                decorationKey = "floor";
                break;
            case InteractionType.Wallpaper:
                decorationKey = "wallpaper";
                break;
            case InteractionType.Landscape:
                decorationKey = "landscape";
                break;
        }
        switch (decorationKey)
        {
            case "floor":
                room.Floor = item.ExtraData;
                _questManager.ProgressUserQuest(session, QuestType.FurniDecoFloor);
                _achievementManager.ProgressAchievement(session, "ACH_RoomDecoFloor", 1);
                break;
            case "wallpaper":
                room.Wallpaper = item.ExtraData;
                _questManager.ProgressUserQuest(session, QuestType.FurniDecoWall);
                _achievementManager.ProgressAchievement(session, "ACH_RoomDecoWallpaper", 1);
                break;
            case "landscape":
                room.Landscape = item.ExtraData;
                _achievementManager.ProgressAchievement(session, "ACH_RoomDecoLandscape", 1);
                break;
        }
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("UPDATE `rooms` SET `" + decorationKey + "` = @extradata WHERE `id` = '" + room.RoomId + "' LIMIT 1");
            dbClient.AddParameter("extradata", item.ExtraData);
            dbClient.RunQuery();
            dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + item.Id + "' LIMIT 1");
        }
        session.GetHabbo().GetInventoryComponent().RemoveItem(item.Id);
        room.SendPacket(new RoomPropertyComposer(decorationKey, item.ExtraData));
        return Task.CompletedTask;
    }
}
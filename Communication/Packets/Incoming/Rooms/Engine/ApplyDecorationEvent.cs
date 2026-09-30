using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.DataFormat;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine;

internal class ApplyDecorationEvent : RoomPacketEvent
{
    private readonly IAchievementManager _achievementManager;
    private readonly IQuestManager _questManager;
    private readonly IDatabase _database;

    public ApplyDecorationEvent(IAchievementManager achievementManager, IQuestManager questManager, IDatabase database)
    {
        _achievementManager = achievementManager;
        _questManager = questManager;
        _database = database;
    }

    public override Task Parse(Room room, GameClient session, IIncomingPacket packet)
    {
        lock (session.GetHabbo().Inventory.Furniture.SyncRoot)
        {
            if (!room.CheckRights(session, true))
                return Task.CompletedTask;
            var item = session.GetHabbo().Inventory.Furniture.GetItem(packet.ReadUInt());
            if (item == null)
                return Task.CompletedTask;
            if (item.Definition == null)
                return Task.CompletedTask;
            var decorationKey = string.Empty;
            switch (item.Definition.InteractionType)
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
            var data = (item.ExtraData is LegacyDataFormat legacyData ? legacyData.Data : string.Empty);
            if (string.IsNullOrWhiteSpace(data))
                return Task.CompletedTask;

            switch (decorationKey)
            {
                case "floor":
                    room.Floor = data;
                    _questManager.ProgressUserQuest(session, QuestType.FurniDecoFloor);
                    _achievementManager.ProgressAchievement(session, "ACH_RoomDecoFloor", 1);
                    break;
                case "wallpaper":
                    room.Wallpaper = data;
                    _questManager.ProgressUserQuest(session, QuestType.FurniDecoWall);
                    _achievementManager.ProgressAchievement(session, "ACH_RoomDecoWallpaper", 1);
                    break;
                case "landscape":
                    room.Landscape = data;
                    _achievementManager.ProgressAchievement(session, "ACH_RoomDecoLandscape", 1);
                    break;
            }
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.SetQuery($"UPDATE `rooms` SET `{decorationKey}` = @extradata WHERE `id` = '{room.RoomId}' LIMIT 1");
                dbClient.AddParameter("extradata", item.ExtraData);
                dbClient.RunQuery();
                dbClient.RunQuery($"DELETE FROM `items` WHERE `id` = '{item.Id}' LIMIT 1");
            }
            session.GetHabbo().Inventory.Furniture.RemoveItem(item.Id);
            session.Send(new FurniListRemoveComposer(item.Id));
            room.SendPacket(new RoomPropertyComposer(decorationKey, data));
            return Task.CompletedTask;
        }
    }
}

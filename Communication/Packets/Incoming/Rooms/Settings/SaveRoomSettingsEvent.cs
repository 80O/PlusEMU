using System.Text;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Settings;
using Plus.Database;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Navigator;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Filter;

namespace Plus.Communication.Packets.Incoming.Rooms.Settings;

internal class SaveRoomSettingsEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IWordFilterManager _wordFilterManager;
    private readonly INavigatorManager _navigationManager;
    private readonly IAchievementManager _achievementManager;
    private readonly IDatabase _database;

    public SaveRoomSettingsEvent(IRoomManager roomManager, IWordFilterManager wordFilterManager, INavigatorManager navigatorManager, IAchievementManager achievementManager, IDatabase database)
    {
        _roomManager = roomManager;
        _wordFilterManager = wordFilterManager;
        _navigationManager = navigatorManager;
        _achievementManager = achievementManager;
        _database = database;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var roomId = packet.ReadUInt();
        if (!_roomManager.TryLoadRoom(roomId, out var room))
            return Task.CompletedTask;
        var name = _wordFilterManager.CheckMessage(packet.ReadString());
        var description = _wordFilterManager.CheckMessage(packet.ReadString());
        var access = RoomAccessUtility.ToRoomAccess(packet.ReadInt());
        var password = packet.ReadString();
        var maxUsers = packet.ReadInt();
        var categoryId = packet.ReadInt();
        var tagCount = packet.ReadInt();
        var tags = new List<string>();
        var formattedTags = new StringBuilder();
        for (var i = 0; i < tagCount; i++)
        {
            if (i > 0) formattedTags.Append(",");
            var tag = packet.ReadString().ToLower();
            tags.Add(tag);
            formattedTags.Append(tag);
        }
        var tradeSettings = packet.ReadInt(); //2 = All can trade, 1 = owner only, 0 = no trading.
        var allowPets = packet.ReadBool();
        var allowPetsEat = packet.ReadBool();
        var roomBlockingEnabled = packet.ReadBool();
        var hidewall = packet.ReadBool();
        var wallThickness = packet.ReadInt();
        var floorThickness = packet.ReadInt();
        var whoMute = packet.ReadInt(); // mute
        var whoKick = packet.ReadInt(); // kick
        var whoBan = packet.ReadInt(); // ban
        var chatMode = packet.ReadInt();
        var chatSize = packet.ReadInt();
        var chatSpeed = packet.ReadInt();
        var chatDistance = packet.ReadInt();
        var extraFlood = packet.ReadInt();
        if (chatMode < 0 || chatMode > 1)
            chatMode = 0;
        if (chatSize < 0 || chatSize > 2)
            chatSize = 0;
        if (chatSpeed < 0 || chatSpeed > 2)
            chatSpeed = 0;
        if (chatDistance < 0)
            chatDistance = 1;
        if (chatDistance > 99)
            chatDistance = 100;
        if (extraFlood < 0 || extraFlood > 2)
            extraFlood = 0;
        if (tradeSettings < 0 || tradeSettings > 2)
            tradeSettings = 0;
        if (whoMute < 0 || whoMute > 1)
            whoMute = 0;
        if (whoKick < 0 || whoKick > 2)
            whoKick = 0;
        if (whoBan < 0 || whoBan > 1)
            whoBan = 0;
        if (wallThickness < -2 || wallThickness > 1)
            wallThickness = 0;
        if (floorThickness < -2 || floorThickness > 1)
            floorThickness = 0;
        if (name.Length < 1)
            return Task.CompletedTask;
        if (name.Length > 60)
            name = name.Substring(0, 60);
        if (access == RoomAccess.Password && password.Length == 0)
            access = RoomAccess.Open;
        if (maxUsers < 0)
            maxUsers = 10;
        if (maxUsers > 50)
            maxUsers = 50;
        if (!_navigationManager.TryGetSearchResultList(categoryId, out var searchResultList))
            categoryId = 36;
        if (searchResultList.CategoryType != NavigatorCategoryType.Category || searchResultList.RequiredRank > session.GetHabbo().Rank ||
            session.GetHabbo().Id != room.OwnerId && session.GetHabbo().Rank >= searchResultList.RequiredRank)
            categoryId = 36;
        if (tagCount > 2)
            return Task.CompletedTask;
        room.AllowPets = allowPets;
        room.AllowPetsEating = allowPetsEat;
        room.RoomBlockingEnabled = roomBlockingEnabled;
        room.Hidewall = hidewall;
        room.Name = name;
        room.Access = access;
        room.Description = description;
        room.Category = categoryId;
        room.Password = password;
        room.WhoCanBan = whoBan;
        room.WhoCanKick = whoKick;
        room.WhoCanMute = whoMute;
        room.ClearTags();
        room.AddTagRange(tags);
        room.UsersMax = maxUsers;
        room.WallThickness = wallThickness;
        room.FloorThickness = floorThickness;
        room.ChatMode = chatMode;
        room.ChatSize = chatSize;
        room.ChatSpeed = chatSpeed;
        room.ChatDistance = chatDistance;
        room.ExtraFlood = extraFlood;
        room.TradeSettings = tradeSettings;
        string accessStr;
        switch (access)
        {
            default:
                accessStr = "open";
                break;
            case RoomAccess.Password:
                accessStr = "password";
                break;
            case RoomAccess.Doorbell:
                accessStr = "locked";
                break;
            case RoomAccess.Invisible:
                accessStr = "invisible";
                break;
        }
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery(
                "UPDATE `rooms` SET `caption` = @caption, `description` = @description, `password` = @password, `category` = @categoryId, `state` = @state, `tags` = @tags, `users_max` = @maxUsers, `allow_pets` = @allowPets, `allow_pets_eat` = @allowPetsEat, `room_blocking_disabled` = @roomBlockingDisabled, `allow_hidewall` = @allowHidewall, `floorthick` = @floorThick, `wallthick` = @wallThick, `mute_settings` = @muteSettings, `kick_settings` = @kickSettings, `ban_settings` = @banSettings, `chat_mode` = @chatMode, `chat_size` = @chatSize, `chat_speed` = @chatSpeed, `chat_extra_flood` = @extraFlood, `chat_hearing_distance` = @chatDistance, `trade_settings` = @tradeSettings WHERE `id` = @roomId LIMIT 1");
            dbClient.AddParameter("categoryId", categoryId);
            dbClient.AddParameter("maxUsers", maxUsers);
            dbClient.AddParameter("allowPets", allowPets);
            dbClient.AddParameter("allowPetsEat", allowPetsEat);
            dbClient.AddParameter("roomBlockingDisabled", roomBlockingEnabled);
            dbClient.AddParameter("allowHidewall", room.Hidewall);
            dbClient.AddParameter("floorThick", room.FloorThickness);
            dbClient.AddParameter("wallThick", room.WallThickness);
            dbClient.AddParameter("muteSettings", room.WhoCanMute);
            dbClient.AddParameter("kickSettings", room.WhoCanKick);
            dbClient.AddParameter("banSettings", room.WhoCanBan);
            dbClient.AddParameter("chatMode", room.ChatMode);
            dbClient.AddParameter("chatSize", room.ChatSize);
            dbClient.AddParameter("chatSpeed", room.ChatSpeed);
            dbClient.AddParameter("extraFlood", room.ExtraFlood);
            dbClient.AddParameter("chatDistance", room.ChatDistance);
            dbClient.AddParameter("tradeSettings", room.TradeSettings);
            dbClient.AddParameter("roomId", room.Id);
            dbClient.AddParameter("caption", room.Name);
            dbClient.AddParameter("description", room.Description);
            dbClient.AddParameter("password", room.Password);
            dbClient.AddParameter("state", accessStr);
            dbClient.AddParameter("tags", formattedTags.ToString());
            dbClient.RunQuery();
        }
        room.GetGameMap().GenerateMaps();
        if (session.GetHabbo().CurrentRoom == null)
        {
            session.Send(new RoomSettingsSavedComposer(room.RoomId));
            session.Send(new RoomInfoUpdatedComposer(room.RoomId));
            session.Send(new RoomVisualizationSettingsComposer(room.WallThickness, room.FloorThickness, Convert.ToBoolean(room.Hidewall)));
        }
        else
        {
            room.SendPacket(new RoomSettingsSavedComposer(room.RoomId));
            room.SendPacket(new RoomInfoUpdatedComposer(room.RoomId));
            room.SendPacket(new RoomVisualizationSettingsComposer(room.WallThickness, room.FloorThickness, Convert.ToBoolean(room.Hidewall)));
        }
        _achievementManager.ProgressAchievement(session, "ACH_SelfModDoorModeSeen", 1);
        _achievementManager.ProgressAchievement(session, "ACH_SelfModWalkthroughSeen", 1);
        _achievementManager.ProgressAchievement(session, "ACH_SelfModChatScrollSpeedSeen", 1);
        _achievementManager.ProgressAchievement(session, "ACH_SelfModChatFloodFilterSeen", 1);
        _achievementManager.ProgressAchievement(session, "ACH_SelfModChatHearRangeSeen", 1);
        return Task.CompletedTask;
    }
}
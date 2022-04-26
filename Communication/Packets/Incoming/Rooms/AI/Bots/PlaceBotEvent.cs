using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Inventory.Bots;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.AI.Speech;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Bots;

internal class PlaceBotEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IDatabase _database;

    public PlaceBotEvent(IRoomManager roomManager, IDatabase database)
    {
        _roomManager = roomManager;
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
        var botId = packet.PopInt();
        var x = packet.PopInt();
        var y = packet.PopInt();
        if (!room.GetGameMap().CanWalk(x, y, false) || !room.GetGameMap().ValidTile(x, y))
        {
            session.SendNotification("You cannot place a bot here!");
            return Task.CompletedTask;
        }
        if (!session.GetHabbo().GetInventoryComponent().TryGetBot(botId, out var bot))
            return Task.CompletedTask;
        var botCount = 0;
        foreach (var user in room.GetRoomUserManager().GetUserList().ToList())
        {
            if (user == null || user.IsPet || !user.IsBot)
                continue;
            botCount += 1;
        }
        if (botCount >= 5 && !session.GetHabbo().GetPermissions().HasRight("bot_place_any_override"))
        {
            session.SendNotification("Sorry; 5 bots per room only!");
            return Task.CompletedTask;
        }

        //TODO: Hmm, maybe not????
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("UPDATE `bots` SET `room_id` = @roomId, `x` = @CoordX, `y` = @CoordY WHERE `id` = @BotId LIMIT 1");
            dbClient.AddParameter("roomId", room.RoomId);
            dbClient.AddParameter("BotId", bot.Id);
            dbClient.AddParameter("CoordX", x);
            dbClient.AddParameter("CoordY", y);
            dbClient.RunQuery();
        }
        var botSpeechList = new List<RandomSpeech>();

        //TODO: Grab data?
        DataRow getData;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `ai_type`,`rotation`,`walk_mode`,`automatic_chat`,`speaking_interval`,`mix_sentences`,`chat_bubble` FROM `bots` WHERE `id` = @BotId LIMIT 1");
            dbClient.AddParameter("BotId", bot.Id);
            getData = dbClient.GetRow();
            dbClient.SetQuery("SELECT `text` FROM `bots_speech` WHERE `bot_id` = @BotId");
            dbClient.AddParameter("BotId", bot.Id);
            var botSpeech = dbClient.GetTable();
            foreach (DataRow speech in botSpeech.Rows) botSpeechList.Add(new RandomSpeech(Convert.ToString(speech["text"]), bot.Id));
        }
        var botUser = room.GetRoomUserManager().DeployBot(
            new RoomBot(bot.Id, session.GetHabbo().CurrentRoomId, Convert.ToString(getData["ai_type"]), Convert.ToString(getData["walk_mode"]), bot.Name, "", bot.Figure, x, y, 0, 4, 0, 0, 0, 0,
                ref botSpeechList, "", 0, bot.OwnerId, ConvertExtensions.EnumToBool(getData["automatic_chat"].ToString()), Convert.ToInt32(getData["speaking_interval"]),
                ConvertExtensions.EnumToBool(getData["mix_sentences"].ToString()), Convert.ToInt32(getData["chat_bubble"])), null);
        botUser.Chat("Hello!");
        room.GetGameMap().UpdateUserMovement(new Point(x, y), new Point(x, y), botUser);
        if (!session.GetHabbo().GetInventoryComponent().TryRemoveBot(botId, out var toRemove))
        {
            Console.WriteLine("Error whilst removing Bot: " + toRemove.Id);
            return Task.CompletedTask;
        }
        session.SendPacket(new BotInventoryComposer(session.GetHabbo().GetInventoryComponent().GetBots()));
        return Task.CompletedTask;
    }
}
﻿using System.Drawing;
using Plus.Communication.Packets.Outgoing.Inventory.Bots;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Inventory.Bots;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Bots;

internal class PickUpBotEvent : IPacketEvent
{
    public readonly IDatabase _database;

    public PickUpBotEvent(IDatabase database)
    {
        _database = database;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var botId = packet.ReadInt();
        if (botId == 0)
            return Task.CompletedTask;
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        if (!room.GetRoomUserManager().TryGetBot(botId, out var botUser))
            return Task.CompletedTask;
        if (session.GetHabbo().Id != botUser.BotData.OwnerId && !session.GetHabbo().GetPermissions().HasRight("bot_place_any_override"))
        {
            session.SendWhisper("You can only pick up your own bots!");
            return Task.CompletedTask;
        }
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("UPDATE `bots` SET `room_id` = '0' WHERE `id` = @id LIMIT 1");
            dbClient.AddParameter("id", botId);
            dbClient.RunQuery();
        }
        room.GetGameMap().RemoveUserFromMap(botUser, new(botUser.X, botUser.Y));
        session.GetHabbo().Inventory.Bots.AddBot(new(Convert.ToInt32(botUser.BotData.Id), Convert.ToInt32(botUser.BotData.OwnerId), botUser.BotData.Name, botUser.BotData.Motto,
            botUser.BotData.Look, botUser.BotData.Gender));
        session.Send(new BotInventoryComposer(session.GetHabbo().Inventory.Bots.Bots.Values.ToList()));
        room.GetRoomUserManager().RemoveBot(botUser.VirtualId, false);
        return Task.CompletedTask;
    }
}
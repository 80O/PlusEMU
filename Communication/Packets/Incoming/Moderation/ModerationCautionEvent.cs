﻿using Plus.Database;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Moderation;

internal class ModerationCautionEvent : IPacketEvent
{
    private readonly IGameClientManager _clientManager;
    private readonly IDatabase _database;

    public ModerationCautionEvent(IGameClientManager clientManager, IDatabase database)
    {
        _clientManager = clientManager;
        _database = database;
    }

    public void Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().GetPermissions().HasRight("mod_caution"))
            return;
        var userId = packet.PopInt();
        var message = packet.PopString();
        var client = _clientManager.GetClientByUserId(userId);
        if (client == null || client.GetHabbo() == null)
            return;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.RunQuery("UPDATE `user_info` SET `cautions` = `cautions` + '1' WHERE `user_id` = '" + client.GetHabbo().Id + "' LIMIT 1");
        }
        client.SendNotification(message);
    }
}
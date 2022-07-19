﻿using Plus.HabboHotel.GameClients;
using Plus.Database;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Users;

internal class SetChatPreferenceEvent : IPacketEvent
{
    private readonly IDatabase _database;

    public SetChatPreferenceEvent(IDatabase database)
    {
        _database = database;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var chatPreference = packet.ReadBool();
        session.GetHabbo().ChatPreference = chatPreference;
        using var dbClient = _database.GetQueryReactor();
        dbClient.SetQuery("UPDATE `users` SET `chat_preference` = @chatPreference WHERE `id` = @habboId LIMIT 1");
        dbClient.AddParameter("habboId", session.GetHabbo().Id);
        dbClient.AddParameter("chatPreference", ConvertExtensions.ToStringEnumValue(chatPreference));
        dbClient.RunQuery();
        return Task.CompletedTask;
    }
}
﻿using Dapper;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;
using Plus.Utilities;

namespace Plus.HabboHotel.Ambassadors;

public class AmbassadorsManager : IAmbassadorsManager
{
    private readonly IDatabase _database;

    public AmbassadorsManager(IDatabase database)
    {
        _database = database;
    }

    public async Task Warn(Habbo ambassador, Habbo target, string message)
    {
        if (!ambassador.Client.GetHabbo().IsAmbassador)
            return;

        using var connection = _database.Connection();
        await connection.ExecuteAsync("INSERT INTO `ambassador_logs` (`user_id`,`target`,`sanctions_type`,`timestamp`) VALUES (@user_id,@target_name,@sanctions_type,@timestamp)",
            new
            {
                user_id = ambassador.Id,
                target_name = target.Username,
                sanctions_type = message,
                timestamp = UnixTimestamp.GetNow()
            });
        ambassador.Client.SendWhisper($"You have successfully warned {target.Username}.");
        target.Client.Send(new RoomNotificationComposer("ambassador.alert.warning", "message", "${notification.ambassador.alert.warning.message}"));
    }
}
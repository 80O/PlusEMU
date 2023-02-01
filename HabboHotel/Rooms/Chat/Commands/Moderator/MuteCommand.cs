﻿using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class MuteCommand : ITargetChatCommand
{
    private readonly IDatabase _database;
    public string Key => "mute";
    public string PermissionRequired => "command_mute";

    public string Parameters => "%username% %time%";

    public string Description => "Mute another user for a certain amount of time.";

    public bool MustBeInSameRoom => false;

    public MuteCommand(IDatabase database)
    {
        _database = database;
    }

    public Task Execute(GameClient session, Room room, Habbo target, string[] parameters)
    {
        if (target.Permissions.HasRight("mod_tool") && !session.GetHabbo().Permissions.HasRight("mod_mute_any"))
        {
            session.SendWhisper("Oops, you cannot mute that user.");
            return Task.CompletedTask;
        }
        if (double.TryParse(parameters[0], out var time))
        {
            if (time > 600 && !session.GetHabbo().Permissions.HasRight("mod_mute_limit_override"))
                time = 600;
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `users` SET `time_muted` = '" + time + "' WHERE `id` = '" + target.Id + "' LIMIT 1");
            }
            if (target.Client != null)
            {
                target.TimeMuted = time;
                target.Client.SendNotification("You have been muted by a moderator for " + time + " seconds!");
            }
            session.SendWhisper("You have successfully muted " + target.Username + " for " + time + " seconds.");
        }
        else
            session.SendWhisper("Please enter a valid integer.");

        return Task.CompletedTask;
    }
}
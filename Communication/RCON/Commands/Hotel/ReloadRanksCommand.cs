﻿namespace Plus.Communication.RCON.Commands.Hotel;

internal class ReloadRanksCommand : IRconCommand
{
    public string Description => "This command is used to reload user permissions.";

    public string Key => "reload_ranks";
    public string Parameters => "";

    public Task<bool> TryExecute(string[] parameters)
    {
        PlusEnvironment.GetGame().GetPermissionManager().Init();
        foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
        {
            if (client == null || client.GetHabbo() == null || client.GetHabbo().Permissions == null)
                continue;
            client.GetHabbo().Permissions.Init(client.GetHabbo());
        }
        return Task.FromResult(true);
    }
}
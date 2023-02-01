﻿using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Permissions;

namespace Plus.Communication.RCON.Commands.Hotel;

internal class ReloadRanksCommand : IRconCommand
{
    private readonly IPermissionManager _permissionManager;
    private readonly IGameClientManager _gameClientManager;
    public string Description => "This command is used to reload user permissions.";

    public string Key => "reload_ranks";
    public string Parameters => "";

    public ReloadRanksCommand(IPermissionManager permissionManager, IGameClientManager gameClientManager)
    {
        _permissionManager = permissionManager;
        _gameClientManager = gameClientManager;
    }
    public Task<bool> TryExecute(string[] parameters)
    {
        _permissionManager.Init();
        foreach (var client in _gameClientManager.GetClients.ToList())
        {
            if (client?.GetHabbo() == null || client.GetHabbo().Permissions == null)
                continue;
            client.GetHabbo().Permissions.Init(client.GetHabbo());
        }
        return Task.FromResult(true);
    }
}
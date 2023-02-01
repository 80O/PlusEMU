﻿using Plus.Database;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.RCON.Commands.User;

internal class ReloadUserVipRankCommand : IRconCommand
{
    private readonly IDatabase _database;
    private readonly IGameClientManager _gameClientManager;
    public string Description => "This command is used to reload a users VIP rank and permissions.";

    public string Key => "reload_user_vip_rank";
    public string Parameters => "%userId%";

    public ReloadUserVipRankCommand(IDatabase database, IGameClientManager gameClientManager)
    {
        _database = database;
        _gameClientManager = gameClientManager;
    }

    public Task<bool> TryExecute(string[] parameters)
    {
        if (!int.TryParse(parameters[0], out var userId))
            return Task.FromResult(false);
        var client = _gameClientManager.GetClientByUserId(userId);
        if (client == null || client.GetHabbo() == null)
            return Task.FromResult(false);
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `rank_vip` FROM `users` WHERE `id` = @userId LIMIT 1");
            dbClient.AddParameter("userId", userId);
            client.GetHabbo().VipRank = dbClient.GetInteger();
        }
        client.GetHabbo().Permissions.Init(client.GetHabbo());
        return Task.FromResult(true);
    }
}
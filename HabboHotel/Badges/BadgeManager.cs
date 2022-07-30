using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Plus.Communication.Packets.Outgoing.Inventory.Badges;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Database;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Badges;

namespace Plus.HabboHotel.Badges;

public class BadgeManager : IBadgeManager
{
    private readonly IDatabase _database;
    private readonly ILogger<BadgeManager> _logger;

    private readonly Dictionary<string, BadgeDefinition> _badges;

    public BadgeManager(IDatabase database, ILogger<BadgeManager> logger)
    {
        _database = database;
        _badges = new Dictionary<string, BadgeDefinition>();
        _logger = logger;
    }

    public void Init()
    {
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT * FROM `badge_definitions`;");
            var data = dbClient.GetTable();
            foreach (DataRow row in data.Rows)
            {
                var code = Convert.ToString(row["code"]).ToUpper();
                if (!_badges.ContainsKey(code))
                    _badges.Add(code, new BadgeDefinition(code, Convert.ToString(row["required_right"])));
            }
        }
        _logger.LogInformation("Loaded " + _badges.Count + " badge definitions.");
    }

    public bool TryGetBadge(string code, out BadgeDefinition badge) => _badges.TryGetValue(code.ToUpper(), out badge);

    public async Task GiveBadge(Habbo habbo, string code)
    {
        if (habbo.Inventory.Badges.HasBadge(code))
            return;
        BadgeDefinition badge = null;
        if (!TryGetBadge(code.ToUpper(), out badge) || badge.RequiredRight.Length > 0 && !habbo.GetPermissions().HasRight(badge.RequiredRight))
            return;

        using var connection = _database.Connection();
        await connection.ExecuteScalarAsync<int>("REPLACE INTO `user_badges` (`user_id`,`badge_id`,`badge_slot`) VALUES (@userId, @badge, '0');", new
        {
            userId = habbo.Id,
            badge = badge.Code
        });
        habbo.Inventory.Badges.AddBadge(new Badge(code, 0));
            habbo.GetClient().Send(new BadgesComposer(habbo.GetClient()));
            habbo.GetClient().Send(new FurniListNotificationComposer(1, 4));
    }

    public async Task RemoveBadge(Habbo habbo, string badge)
    {
        if (!habbo.Inventory.Badges.HasBadge(badge)) return;
        using var connection = _database.Connection();
        await connection.ExecuteAsync("DELETE FROM user_badges WHERE badge_id = @badge AND user_id = @userId LIMIT 1", new
        {
            badge,
            userId = habbo.Id
        });
        habbo.Inventory.Badges.RemoveBadge(badge);
    }

    public async Task<List<Badge>> LoadBadgesForHabbo(int userId)
    {
        using var connection = _database.Connection();
        return (await connection.QueryAsync<Badge>("SELECT badge_id as code, badge_slot as slot FROM user_badges WHERE user_id = @userId", new { userId })).ToList();
    }
}
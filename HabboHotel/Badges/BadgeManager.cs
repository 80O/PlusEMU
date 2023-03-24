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

    private Dictionary<string, BadgeDefinition> _badges;
    public IReadOnlyDictionary<string, BadgeDefinition> Badges => _badges;

    public BadgeManager(IDatabase database, ILogger<BadgeManager> logger)
    {
        _database = database;
        _logger = logger;
        _badges = new();
    }

    public async Task Init()
    {
        using var connection = _database.Connection();
        _badges = (await connection.QueryAsync<BadgeDefinition>("SELECT * FROM badge_definitions")).ToDictionary(definition => definition.Code.ToUpper());
        _logger.LogInformation("Loaded " + Badges.Count + " badge definitions.");
    }

    public async Task GiveBadge(Habbo habbo, string code)
    {
        if (habbo.Inventory.Badges.HasBadge(code))
            return;

        if (!_badges.TryGetValue(code.ToUpper(), out var badge) || badge.RequiredRight.Length > 0 && !habbo.Permissions.HasRight(badge.RequiredRight))
            return;

        using var connection = _database.Connection();
        await connection.ExecuteScalarAsync<int>("REPLACE INTO `user_badges` (`user_id`,`badge_id`,`badge_slot`) VALUES (@userId, @badge, '0');", new
        {
            userId = habbo.Id,
            badge = badge.Code
        });
        habbo.Inventory.Badges.AddBadge(new(code, 0));

        habbo.Client.Send(new BadgesComposer(habbo.Id, habbo.Inventory.Badges.Badges));
        habbo.Client.Send(new FurniListNotificationComposer(1, 4));
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
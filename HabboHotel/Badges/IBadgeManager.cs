using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Badges;

namespace Plus.HabboHotel.Badges;

public interface IBadgeManager
{
    void Init();
    bool TryGetBadge(string code, out BadgeDefinition badge);
    Task GiveBadge(Habbo habbo, string code);
    Task RemoveBadge(Habbo habbo, string badge);
    Task<List<Badge>> LoadBadgesForHabbo(int userId);
}
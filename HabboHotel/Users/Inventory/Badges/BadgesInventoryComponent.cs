using Plus.HabboHotel.Users.Badges;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Users.Inventory.Badges
{
    public class BadgesInventoryComponent
    {
        private readonly ConcurrentDictionary<string, Badge> _badges;

        public IReadOnlyDictionary<string, Badge> Badges => _badges;

        public BadgesInventoryComponent(Dictionary<string, Badge> badges)
        {
            _badges = new(badges);
        }

        public Badge? GetBadge(string badge)
        {
            if (_badges.TryGetValue(badge, out var b))
                return b;
            return null;
        }

        public bool HasBadge(string badge) => _badges.ContainsKey(badge);

        public void AddBadge(Badge badge) => _badges.TryAdd(badge.Code, badge);

        public void RemoveBadge(string badge) => _badges.TryRemove(badge, out _);

        public List<Badge> EquippedBadges => _badges.Values.Where(badge => badge.Slot > 0).ToList();

        public void ClearWearingBadges()
        {
            foreach (var (_, badge) in _badges)
                badge.Slot = 0;
        }
    }
}

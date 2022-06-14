using Plus.HabboHotel.Badges;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Users.Inventory.Bots;
using Plus.HabboHotel.Users.Inventory.Pets;
using Plus.HabboHotel.Users.UserData;

namespace Plus.HabboHotel.Users.Inventory
{
    internal class LoadUserInventoryTask : IUserDataLoadingTask
    {
        private readonly IBadgeManager _badgeManager;

        public LoadUserInventoryTask(IBadgeManager badgeManager)
        {
            _badgeManager = badgeManager;
        }

        public async Task Load(Habbo habbo)
        {
            var items = ItemLoader.GetItemsForUser(habbo.Id);
            habbo.SetInventoryComponent(new()
            {
                UserId = habbo.Id,
                Badges = new((await _badgeManager.LoadBadgesForHabbo(habbo.Id)).ToDictionary(badge => badge.Code)),
                Furniture = new(items.Where(i => i.IsFloorItem).ToList(), items.Where(i => i.IsWallItem).ToList()),
                Pets = new(PetLoader.GetPetsForUser(habbo.Id)),
                Bots = new(BotLoader.GetBotsForUser(habbo.Id))
            });
        }
    }
}

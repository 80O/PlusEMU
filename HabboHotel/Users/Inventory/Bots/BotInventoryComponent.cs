using System.Collections.Concurrent;

namespace Plus.HabboHotel.Users.Inventory.Bots
{
    public class BotInventoryComponent
    {
        private ConcurrentDictionary<int, Bot> _bots;

        public IReadOnlyDictionary<int, Bot> Bots => _bots;

        public BotInventoryComponent(List<Bot> bots)
        {
            _bots = new(bots.ToDictionary(bot => bot.Id));
        }

        public bool AddBot(Bot bot) => _bots.TryAdd(bot.Id, bot);

        public bool RemoveBot(int botId) => _bots.TryRemove(botId, out _);
    }
}

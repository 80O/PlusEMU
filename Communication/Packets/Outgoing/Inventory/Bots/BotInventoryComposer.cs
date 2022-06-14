using Plus.HabboHotel.Users.Inventory.Bots;

namespace Plus.Communication.Packets.Outgoing.Inventory.Bots;

internal class BotInventoryComposer : ServerPacket
{
    public BotInventoryComposer(ICollection<Bot> bots)
        : base(ServerPacketHeader.BotInventoryMessageComposer)
    {
        WriteInteger(bots.Count);
        foreach (var bot in bots.ToList())
        {
            WriteInteger(bot.Id);
            WriteString(bot.Name);
            WriteString(bot.Motto);
            WriteString(bot.Gender);
            WriteString(bot.Figure);
        }
    }
}
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Inventory.Purse;

internal class GetHabboClubWindowEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        // Session.SendNotification("Habbo Club is free for all members, enjoy!");
        return Task.CompletedTask;
    }
}
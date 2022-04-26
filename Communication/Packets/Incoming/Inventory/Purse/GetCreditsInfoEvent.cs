using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Inventory.Purse;

internal class GetCreditsInfoEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
        session.SendPacket(new ActivityPointsComposer(session.GetHabbo().Duckets, session.GetHabbo().Diamonds, session.GetHabbo().GotwPoints));
        return Task.CompletedTask;
    }
}
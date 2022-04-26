using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Incoming.Catalog;

internal class GetGroupFurniConfigEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;

    public GetGroupFurniConfigEvent(IGroupManager groupManager)
    {
        _groupManager = groupManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        session.SendPacket(new GroupFurniConfigComposer(_groupManager.GetGroupsForUser(session.GetHabbo().Id)));
        return Task.CompletedTask;
    }
}
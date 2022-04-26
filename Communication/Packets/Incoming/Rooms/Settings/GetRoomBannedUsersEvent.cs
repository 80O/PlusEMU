using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Settings;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Settings;

internal class GetRoomBannedUsersEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var instance = session.GetHabbo().CurrentRoom;
        if (instance == null || !instance.CheckRights(session, true))
            return Task.CompletedTask;
        if (instance.GetBans().BannedUsers().Count > 0)
            session.SendPacket(new GetRoomBannedUsersComposer(instance));
        return Task.CompletedTask;
    }
}
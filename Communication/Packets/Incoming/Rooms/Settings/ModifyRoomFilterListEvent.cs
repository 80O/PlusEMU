using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Settings;

internal class ModifyRoomFilterListEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var instance = session.GetHabbo().CurrentRoom;
        if (instance == null)
            return Task.CompletedTask;
        if (!instance.CheckRights(session))
            return Task.CompletedTask;
        packet.PopInt(); //roomId
        var added = packet.PopBoolean();
        var word = packet.PopString();
        if (added)
            instance.GetFilter().AddFilter(word);
        else
            instance.GetFilter().RemoveFilter(word);
        return Task.CompletedTask;
    }
}
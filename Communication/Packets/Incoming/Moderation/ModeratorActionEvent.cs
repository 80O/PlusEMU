using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Moderation;

internal class ModeratorActionEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().GetPermissions().HasRight("mod_caution"))
            return Task.CompletedTask;
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var currentRoom = session.GetHabbo().CurrentRoom;
        if (currentRoom == null)
            return Task.CompletedTask;
        var alertMode = packet.PopInt();
        var alertMessage = packet.PopString();
        var isCaution = alertMode != 3;
        alertMessage = isCaution ? "Caution from Moderator:\n\n" + alertMessage : "Message from Moderator:\n\n" + alertMessage;
        session.GetHabbo().CurrentRoom.SendPacket(new BroadcastMessageAlertComposer(alertMessage));
        return Task.CompletedTask;
    }
}
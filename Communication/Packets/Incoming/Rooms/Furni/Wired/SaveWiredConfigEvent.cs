using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Furni.Wired;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items.Wired;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Wired;

internal abstract class SaveWiredConfigEvent : IPacketEvent
{
    public virtual Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var room = session.GetHabbo().CurrentRoom;
        if (room == null || !room.CheckRights(session, false, true))
            return Task.CompletedTask;
        var itemId = packet.PopInt();
        session.SendPacket(new HideWiredConfigComposer());
        var selectedItem = room.GetRoomItemHandler().GetItem(itemId);
        if (selectedItem == null)
            return Task.CompletedTask;
        if (!session.GetHabbo().CurrentRoom.GetWired().TryGet(itemId, out var box))
            return Task.CompletedTask;
        if (box.Type == WiredBoxType.EffectGiveUserBadge && !session.GetHabbo().GetPermissions().HasRight("room_item_wired_rewards"))
        {
            session.SendNotification("You don't have the correct permissions to do this.");
            return Task.CompletedTask;
        }
        box.HandleSave(packet);
        session.GetHabbo().CurrentRoom.GetWired().SaveBox(box);
        return Task.CompletedTask;
    }
}
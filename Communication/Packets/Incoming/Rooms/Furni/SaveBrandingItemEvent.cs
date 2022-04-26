using System;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni;

internal class SaveBrandingItemEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        if (!room.CheckRights(session, true) || !session.GetHabbo().GetPermissions().HasRight("room_item_save_branding_items"))
            return Task.CompletedTask;
        var itemId = packet.PopInt();
        var item = room.GetRoomItemHandler().GetItem(itemId);
        if (item == null)
            return Task.CompletedTask;
        if (item.Data.InteractionType == InteractionType.Background)
        {
            var data = packet.PopInt();
            var brandData = "state" + Convert.ToChar(9) + "0";
            for (var i = 1; i <= data; i++) brandData = brandData + Convert.ToChar(9) + packet.PopString();
            item.ExtraData = brandData;
        }
        else if (item.Data.InteractionType == InteractionType.FxProvider)
        {
            /*int Unknown = Packet.PopInt();
            string Data = Packet.PopString();
            int EffectId = Packet.PopInt();

            Item.ExtraData = Convert.ToString(EffectId);*/
        }
        room.GetRoomItemHandler().SetFloorItem(session, item, item.GetX, item.GetY, item.Rotation, false, false, true);
        return Task.CompletedTask;
    }
}
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.Utilities;

namespace Plus.Communication.Packets.Outgoing.Rooms.Engine;

internal class ObjectsComposer : ServerPacket
{
    public ObjectsComposer(Item[] objects, Room room)
        : base(ServerPacketHeader.ObjectsMessageComposer)
    {
        WriteInteger(1);
        WriteInteger(room.OwnerId);
        WriteString(room.OwnerName);
        WriteInteger(objects.Length);
        foreach (var item in objects) WriteFloorItem(item, Convert.ToInt32(item.UserId));
    }

    private void WriteFloorItem(Item item, int userId)
    {
        WriteInteger(item.Id);
        WriteInteger(item.GetBaseItem().SpriteId);
        WriteInteger(item.GetX);
        WriteInteger(item.GetY);
        WriteInteger(item.Rotation);
        WriteString(TextHandling.GetString(item.GetZ));
        WriteString(string.Empty);
        if (item.LimitedNo > 0)
        {
            WriteInteger(1);
            WriteInteger(256);
            WriteString(item.ExtraData);
            WriteInteger(item.LimitedNo);
            WriteInteger(item.LimitedTot);
        }
        else
            ItemBehaviourUtility.GenerateExtradata(item, this);
        WriteInteger(-1); // to-do: check
        WriteInteger(item.GetBaseItem().Modes > 1 ? 1 : 0);
        WriteInteger(userId);
    }
}
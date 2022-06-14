using Plus.HabboHotel.Catalog.Utilities;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Outgoing.Inventory.Furni;

internal class FurniListComposer : ServerPacket
{
    public FurniListComposer(ICollection<Item> items, int pages, int page)
        : base(ServerPacketHeader.FurniListMessageComposer)
    {
        WriteInteger(pages); //Pages
        WriteInteger(page); //Page?
        WriteInteger(items.Count);
        foreach (var item in items) WriteItem(item);
    }

    private void WriteItem(Item item)
    {
        WriteInteger(item.Id);
        WriteString(item.GetBaseItem().Type.ToString().ToUpper());
        WriteInteger(item.Id);
        WriteInteger(item.GetBaseItem().SpriteId);
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
        WriteBoolean(item.GetBaseItem().AllowEcotronRecycle);
        WriteBoolean(item.GetBaseItem().AllowTrade);
        WriteBoolean(item.LimitedNo == 0 ? item.GetBaseItem().AllowInventoryStack : false);
        WriteBoolean(ItemUtility.IsRare(item));
        WriteInteger(-1); //Seconds to expiration.
        WriteBoolean(true);
        WriteInteger(-1); //Item RoomId
        if (!item.IsWallItem)
        {
            WriteString(string.Empty);
            WriteInteger(0);
        }
    }
}
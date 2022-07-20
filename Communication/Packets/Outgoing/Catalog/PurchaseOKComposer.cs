﻿using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Outgoing.Catalog;

public class PurchaseOkComposer : IServerPacket
{
    private readonly CatalogItem? _item;
    private readonly ItemData? _baseItem;
    public uint MessageId => ServerPacketHeader.PurchaseOkComposer;

    public PurchaseOkComposer()
    {
    }

    public PurchaseOkComposer(CatalogItem item, ItemData baseItem)
    {
        _item = item;
        _baseItem = baseItem;
    }

    public void Compose(IOutgoingPacket packet)
    {
        if (_item != null && _baseItem != null)
        {

            packet.WriteInteger(_baseItem.Id);
            packet.WriteString(_baseItem.ItemName);
            packet.WriteBoolean(false);
            packet.WriteInteger(_item.CostCredits);
            packet.WriteInteger(_item.CostPixels);
            packet.WriteInteger(0);
            packet.WriteBoolean(true);
            packet.WriteInteger(1);
            packet.WriteString(_baseItem.Type.ToString().ToLower());
            packet.WriteInteger(_baseItem.SpriteId);
            packet.WriteString("");
            packet.WriteInteger(1);
            packet.WriteInteger(0);
            packet.WriteString("");
            packet.WriteInteger(1);
        }
        else
        {
            packet.WriteInteger(0);
            packet.WriteString("");
            packet.WriteBoolean(false);
            packet.WriteInteger(0);
            packet.WriteInteger(0);
            packet.WriteInteger(0);
            packet.WriteBoolean(true);
            packet.WriteInteger(1);
            packet.WriteString("s");
            packet.WriteInteger(0);
            packet.WriteString("");
            packet.WriteInteger(1);
            packet.WriteInteger(0);
            packet.WriteString("");
            packet.WriteInteger(1);
        }
    }
}
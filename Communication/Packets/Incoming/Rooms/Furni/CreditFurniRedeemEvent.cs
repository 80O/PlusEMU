﻿using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Core.Settings;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni;

internal class CreditFurniRedeemEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly ISettingsManager _settingsManager;
    private readonly IDatabase _database;

    public CreditFurniRedeemEvent(IRoomManager roomManager, ISettingsManager settingsManager, IDatabase database)
    {
        _roomManager = roomManager;
        _settingsManager = settingsManager;
        _database = database;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        if (!room.CheckRights(session, true))
            return Task.CompletedTask;
        if (_settingsManager.TryGetValue("room.item.exchangeables.enabled") != "1")
        {
            session.SendNotification("The hotel managers have temporarilly disabled exchanging!");
            return Task.CompletedTask;
        }
        var exchange = room.GetRoomItemHandler().GetItem(packet.ReadUInt());
        if (exchange == null)
            return Task.CompletedTask;
        if (exchange.Definition.InteractionType != InteractionType.Exchange)
            return Task.CompletedTask;
        var value = exchange.Definition.BehaviourData;
        if (value > 0)
        {
            session.GetHabbo().Credits += value;
            session.Send(new CreditBalanceComposer(session.GetHabbo().Credits));
        }
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("DELETE FROM `items` WHERE `id` = @exchangeId LIMIT 1");
            dbClient.AddParameter("exchangeId", exchange.Id);
            dbClient.RunQuery();
        }
        session.Send(new FurniListUpdateComposer());
        room.GetRoomItemHandler().RemoveFurniture(null, exchange.Id);
        session.GetHabbo().Inventory.Furniture.RemoveItem(exchange.Id);
        session.Send(new FurniListRemoveComposer(exchange.Id));
        return Task.CompletedTask;
    }
}
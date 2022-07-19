﻿using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.Catalog.Pets;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Incoming.Catalog;

public class GetSellablePetBreedsEvent : IPacketEvent
{
    private readonly IItemDataManager _itemDataManager;
    private readonly IPetRaceManager _petRaceManager;

    public GetSellablePetBreedsEvent(IItemDataManager itemDataManager, IPetRaceManager petRaceManager)
    {
        _itemDataManager = itemDataManager;
        _petRaceManager = petRaceManager;
    }
    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var type = packet.ReadString();
        var item = _itemDataManager.GetItemByName(type);
        if (item == null)
            return Task.CompletedTask;
        var petId = item.BehaviourData;
        session.Send(new SellablePetBreedsComposer(type, petId, _petRaceManager.GetRacesForRaceId(petId)));
        return Task.CompletedTask;
    }
}
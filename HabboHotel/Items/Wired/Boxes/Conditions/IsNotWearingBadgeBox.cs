﻿using System.Collections.Concurrent;
using System.Linq;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Items.Wired.Boxes.Conditions;

internal class IsNotWearingBadgeBox : IWiredItem
{
    public IsNotWearingBadgeBox(Room instance, Item item)
    {
        Instance = instance;
        Item = item;
        SetItems = new ConcurrentDictionary<int, Item>();
    }

    public Room Instance { get; set; }
    public Item Item { get; set; }
    public WiredBoxType Type => WiredBoxType.ConditionIsWearingBadge;
    public ConcurrentDictionary<int, Item> SetItems { get; set; }
    public string StringData { get; set; }
    public bool BoolData { get; set; }
    public string ItemsData { get; set; }

    public void HandleSave(ClientPacket packet)
    {
        var unknown = packet.PopInt();
        var badgeCode = packet.PopString();
        StringData = badgeCode;
    }

    public bool Execute(params object[] @params)
    {
        if (@params.Length == 0)
            return false;
        if (string.IsNullOrEmpty(StringData))
            return false;
        var player = (Habbo)@params[0];
        if (player == null)
            return false;
        if (!player.Inventory.Badges.HasBadge(StringData))
            return true;
        if (!player.Inventory.Badges.EquippedBadges.Any())
            return true;

        return player.Inventory.Badges.EquippedBadges.All(badge => !badge.Code.Equals(StringData));
    }
}
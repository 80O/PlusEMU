﻿using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using System;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    class BotGivesHandItemBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type { get { return WiredBoxType.EffectBotGivesHanditemBox; } }
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public BotGivesHandItemBox(Room instance, Item item)
        {
            this.Instance = instance;
            this.Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket packet)
        {
            var unknown = packet.PopInt();
            var drinkId = packet.PopInt();
            var botName = packet.PopString();

            if (SetItems.Count > 0)
                SetItems.Clear();

            StringData = botName.ToString() + ";" + drinkId.ToString();
        }

        public bool Execute(params object[] @params)
        {
            if (@params == null || @params.Length == 0)
                return false;

            if (String.IsNullOrEmpty(StringData))
                return false;

            var player = (Habbo)@params[0];

            if(player == null)
                return false;

            var actor = Instance.GetRoomUserManager().GetRoomUserByHabbo(player.Id);

            if(actor == null)
                return false;

            var user = Instance.GetRoomUserManager().GetBotByName(StringData.Split(';')[0]);

            if (user == null)
                return false;

            if (user.BotData.TargetUser == 0)
            {
                if (!Instance.GetGameMap().CanWalk(actor.SquareBehind.X, actor.SquareBehind.Y, false))
                    return false;

                var data = StringData.Split(';');

                int drinkId;

                if (!int.TryParse(data[1], out drinkId))
                    return false;

                user.CarryItem(drinkId);
                user.BotData.TargetUser = actor.HabboId;

                user.MoveTo(actor.SquareBehind.X, actor.SquareBehind.Y);
            }

            return true;
        }
    }
}

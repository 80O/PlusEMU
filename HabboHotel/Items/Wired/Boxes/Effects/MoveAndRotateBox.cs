﻿using System;
using System.Linq;
using System.Collections.Concurrent;

using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using System.Drawing;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Utilities;


namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class MoveAndRotateBox : IWiredItem, IWiredCycle
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }

        public WiredBoxType Type
        {
            get { return WiredBoxType.EffectMoveAndRotate; }
        }

        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }

        public int Delay
        {
            get { return _delay; }
            set
            {
                _delay = value;
                TickCount = value + 1;
            }
        }

        public int TickCount { get; set; }
        public string ItemsData { get; set; }
        private bool _requested;
        private int _delay = 0;
        private long _next = 0;

        public MoveAndRotateBox(Room instance, Item item)
        {
            this.Instance = instance;
            this.Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
            TickCount = Delay;
            _requested = false;
        }

        public void HandleSave(ClientPacket packet)
        {
            if (SetItems.Count > 0)
                SetItems.Clear();

            var unknown = packet.PopInt();
            var movement = packet.PopInt();
            var rotation = packet.PopInt();

            var unknown1 = packet.PopString();

            var furniCount = packet.PopInt();
            for (var i = 0; i < furniCount; i++)
            {
                var selectedItem = Instance.GetRoomItemHandler().GetItem(packet.PopInt());

                if (selectedItem != null && !Instance.GetWired().OtherBoxHasItem(this, selectedItem.Id))
                    SetItems.TryAdd(selectedItem.Id, selectedItem);
            }

            StringData = movement + ";" + rotation;
            Delay = packet.PopInt();
        }

        public bool Execute(params object[] @params)
        {
            if (SetItems.Count == 0)
                return false;

            if (_next == 0 || _next < PlusEnvironment.Now())
                _next = PlusEnvironment.Now() + Delay;

            if (!_requested)
            {
                TickCount = Delay;
                _requested = true;
            }
            return true;
        }

        public bool OnCycle()
        {
            if (Instance == null || !_requested || _next == 0)
                return false;

            var now = PlusEnvironment.Now();
            if (_next < now)
            {
                foreach (var item in SetItems.Values.ToList())
                {
                    if (item == null)
                        continue;

                    if (!Instance.GetRoomItemHandler().GetFloor.Contains(item))
                        continue;

                    Item toRemove = null;

                    if (Instance.GetWired().OtherBoxHasItem(this, item.Id))
                        SetItems.TryRemove(item.Id, out toRemove);
   

                    var point = HandleMovement(Convert.ToInt32(StringData.Split(';')[0]),new Point(item.GetX, item.GetY));
                    var newRot = HandleRotation(Convert.ToInt32(StringData.Split(';')[1]), item.Rotation);

                    Instance.GetWired().OnUserFurniCollision(Instance, item);

                    if (!Instance.GetGameMap().ItemCanMove(item, point))
                        continue;

                    if (Instance.GetGameMap().CanRollItemHere(point.X, point.Y) && !Instance.GetGameMap().SquareHasUsers(point.X, point.Y))
                    {
                        var newZ = Instance.GetGameMap().GetHeightForSquareFromData(point);
                        var canBePlaced = true;

                        var coordinatedItems = Instance.GetGameMap().GetCoordinatedItems(point);
                        foreach (var coordinatedItem in coordinatedItems.ToList())
                        {
                            if (coordinatedItem == null || coordinatedItem.Id == item.Id)
                                continue;

                            if (!coordinatedItem.GetBaseItem().Walkable)
                            {
                                _next = 0;
                                canBePlaced = false;
                                break;
                            }

                            if (coordinatedItem.TotalHeight > newZ)
                                newZ = coordinatedItem.TotalHeight;

                            if (canBePlaced == true && !coordinatedItem.GetBaseItem().Stackable)
                                canBePlaced = false;
                        }

                        if (newRot != item.Rotation)
                        {
                            item.Rotation = newRot;
                            item.UpdateState(false, true);
                        }

                        if (canBePlaced && point != item.Coordinate)
                        {
                            Instance.SendPacket(new SlideObjectBundleComposer(item.GetX, item.GetY, item.GetZ, point.X,
                                point.Y, newZ, 0, 0, item.Id));
                            Instance.GetRoomItemHandler().SetFloorItem(item, point.X, point.Y, newZ);
                        }
                    }
                }

                _next = 0;
                return true;
            }
            return false;
        }

        private int HandleRotation(int mode, int rotation)
        {
            switch (mode)
            {
                case 1:
                {
                    rotation += 2;
                    if (rotation > 6)
                    {
                        rotation = 0;
                    }
                    break;
                }

                case 2:
                {
                    rotation -= 2;
                    if (rotation < 0)
                    {
                        rotation = 6;
                    }
                    break;
                }

                case 3:
                {
                    if (RandomNumber.GenerateRandom(0, 2) == 0)
                    {
                        rotation += 2;
                        if (rotation > 6)
                        {
                            rotation = 0;
                        }
                    }
                    else
                    {
                        rotation -= 2;
                        if (rotation < 0)
                        {
                            rotation = 6;
                        }
                    }
                    break;
                }
            }
            return rotation;
        }

        private Point HandleMovement(int mode, Point position)
        {
            var newPos = new Point();
            switch (mode)
            {
                case 0:
                {
                    newPos = position;
                    break;
                }
                case 1:
                {
                    switch (RandomNumber.GenerateRandom(1, 4))
                    {
                        case 1:
                            newPos = new Point(position.X + 1, position.Y);
                            break;
                        case 2:
                            newPos = new Point(position.X - 1, position.Y);
                            break;
                        case 3:
                            newPos = new Point(position.X, position.Y + 1);
                            break;
                        case 4:
                            newPos = new Point(position.X, position.Y - 1);
                            break;
                    }
                    break;
                }
                case 2:
                {
                    if (RandomNumber.GenerateRandom(0, 2) == 1)
                    {
                        newPos = new Point(position.X - 1, position.Y);
                    }
                    else
                    {
                        newPos = new Point(position.X + 1, position.Y);
                    }
                    break;
                }
                case 3:
                {
                    if (RandomNumber.GenerateRandom(0, 2) == 1)
                    {
                        newPos = new Point(position.X, position.Y - 1);
                    }
                    else
                    {
                        newPos = new Point(position.X, position.Y + 1);
                    }
                    break;
                }
                case 4:
                {
                    newPos = new Point(position.X, position.Y - 1);
                    break;
                }
                case 5:
                {
                    newPos = new Point(position.X + 1, position.Y);
                    break;
                }
                case 6:
                {
                    newPos = new Point(position.X, position.Y + 1);
                    break;
                }
                case 7:
                {
                    newPos = new Point(position.X - 1, position.Y);
                    break;
                }
            }

            return newPos;
        }
    }
}
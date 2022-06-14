using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Core;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Data.Moodlight;
using Plus.HabboHotel.Items.Data.Toner;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Rooms.PathFinding;
using Plus.Utilities;
using System.Collections.Concurrent;
using System.Drawing;

namespace Plus.HabboHotel.Rooms;

public class RoomItemHandling
{
    private readonly ConcurrentDictionary<int, Item> _floorItems;

    private readonly ConcurrentDictionary<int, Item> _movedItems;
    private readonly List<int> _rollerItemsMoved;
    private readonly List<ServerPacket> _rollerMessages;

    private readonly ConcurrentDictionary<int, Item> _rollers;
    private readonly List<int> _rollerUsersMoved;
    private readonly Room _room;
    private readonly ConcurrentDictionary<int, Item> _wallItems;
    private int _mRollerCycle;
    private int _mRollerSpeed;

    private ConcurrentQueue<Item> _roomItemUpdateQueue;

    public int HopperCount;

    public RoomItemHandling(Room room)
    {
        _room = room;
        HopperCount = 0;
        GotRollers = false;
        _mRollerSpeed = 4;
        _mRollerCycle = 0;
        _movedItems = new ConcurrentDictionary<int, Item>();
        _rollers = new ConcurrentDictionary<int, Item>();
        _wallItems = new ConcurrentDictionary<int, Item>();
        _floorItems = new ConcurrentDictionary<int, Item>();
        _rollerItemsMoved = new List<int>();
        _rollerUsersMoved = new List<int>();
        _rollerMessages = new List<ServerPacket>();
        _roomItemUpdateQueue = new ConcurrentQueue<Item>();
    }

    public bool GotRollers { get; set; }

    public ICollection<Item> GetFloor => _floorItems.Values;

    public ICollection<Item> GetWall => _wallItems.Values;

    public IEnumerable<Item> GetWallAndFloor => _floorItems.Values.Concat(_wallItems.Values);

    public void TryAddRoller(int itemId, Item roller)
    {
        _rollers.TryAdd(itemId, roller);
    }

    public void QueueRoomItemUpdate(Item item)
    {
        _roomItemUpdateQueue.Enqueue(item);
    }

    public void SetSpeed(int p)
    {
        _mRollerSpeed = p;
    }

    public string WallPositionCheck(string wallPosition)
    {
        //:w=3,2 l=9,63 l
        try
        {
            if (wallPosition.Contains(Convert.ToChar(13))) return null;
            if (wallPosition.Contains(Convert.ToChar(9))) return null;
            var posD = wallPosition.Split(' ');
            if (posD[2] != "l" && posD[2] != "r")
                return null;
            var widD = posD[0].Substring(3).Split(',');
            var widthX = int.Parse(widD[0]);
            var widthY = int.Parse(widD[1]);
            if (widthX < -1000 || widthY < -1 || widthX > 700 || widthY > 700)
                return null;
            var lenD = posD[1].Substring(2).Split(',');
            var lengthX = int.Parse(lenD[0]);
            var lengthY = int.Parse(lenD[1]);
            if (lengthX < -1 || lengthY < -1000 || lengthX > 700 || lengthY > 700)
                return null;
            return ":w=" + widthX + "," + widthY + " " + "l=" + lengthX + "," + lengthY + " " + posD[2];
        }
        catch
        {
            return null;
        }
    }

    public void LoadFurniture()
    {
        if (_floorItems.Count > 0)
            _floorItems.Clear();
        if (_wallItems.Count > 0)
            _wallItems.Clear();
        var items = ItemLoader.GetItemsForRoom(_room.Id, _room);
        foreach (var item in items.ToList())
        {
            if (item == null)
                continue;
            if (item.UserId == 0)
            {
                using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
                dbClient.SetQuery("UPDATE `items` SET `user_id` = @UserId WHERE `id` = @ItemId LIMIT 1");
                dbClient.AddParameter("ItemId", item.Id);
                dbClient.AddParameter("UserId", _room.OwnerId);
                dbClient.RunQuery();
            }
            if (item.IsFloorItem)
            {
                if (!_room.GetGameMap().ValidTile(item.GetX, item.GetY))
                {
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = '" + item.Id + "' LIMIT 1");
                    }
                    var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(item.UserId);
                    if (client != null)
                    {
                        client.GetHabbo().Inventory.AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, item.LimitedNo, item.LimitedTot);
                        client.SendPacket(new FurniListUpdateComposer());
                    }
                    continue;
                }
                if (!_floorItems.ContainsKey(item.Id))
                    _floorItems.TryAdd(item.Id, item);
            }
            else if (item.IsWallItem)
            {
                if (string.IsNullOrWhiteSpace(item.WallCoord))
                {
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `items` SET `wall_pos` = @WallPosition WHERE `id` = '" + item.Id + "' LIMIT 1");
                        dbClient.AddParameter("WallPosition", ":w=0,2 l=11,53 l");
                        dbClient.RunQuery();
                    }
                    item.WallCoord = ":w=0,2 l=11,53 l";
                }
                try
                {
                    item.WallCoord = WallPositionCheck(":" + item.WallCoord.Split(':')[1]);
                }
                catch
                {
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `items` SET `wall_pos` = @WallPosition WHERE `id` = '" + item.Id + "' LIMIT 1");
                        dbClient.AddParameter("WallPosition", ":w=0,2 l=11,53 l");
                        dbClient.RunQuery();
                    }
                    item.WallCoord = ":w=0,2 l=11,53 l";
                }
                if (!_wallItems.ContainsKey(item.Id))
                    _wallItems.TryAdd(item.Id, item);
            }
        }
        foreach (var item in _floorItems.Values.ToList())
        {
            if (item.IsRoller)
                GotRollers = true;
            else if (item.GetBaseItem().InteractionType == InteractionType.Moodlight)
            {
                if (_room.MoodlightData == null)
                    _room.MoodlightData = new MoodlightData(item.Id);
            }
            else if (item.GetBaseItem().InteractionType == InteractionType.Toner)
            {
                if (_room.TonerData == null)
                    _room.TonerData = new TonerData(item.Id);
            }
            else if (item.IsWired)
            {
                if (_room == null)
                    continue;
                if (_room.GetWired() == null)
                    continue;
                _room.GetWired().LoadWiredBox(item);
            }
            else if (item.GetBaseItem().InteractionType == InteractionType.Hopper)
                HopperCount++;
        }
    }

    public Item GetItem(int pId)
    {
        if (_floorItems != null && _floorItems.ContainsKey(pId))
        {
            Item item = null;
            if (_floorItems.TryGetValue(pId, out item))
                return item;
        }
        else if (_wallItems != null && _wallItems.ContainsKey(pId))
        {
            Item item = null;
            if (_wallItems.TryGetValue(pId, out item))
                return item;
        }
        return null;
    }

    public void RemoveFurniture(GameClient session, int id)
    {
        var item = GetItem(id);
        if (item == null)
            return;
        if (item.GetBaseItem().InteractionType == InteractionType.FootballGate)
            _room.GetSoccer().UnRegisterGate(item);
        if (item.GetBaseItem().InteractionType != InteractionType.Gift)
            item.Interactor.OnRemove(session, item);
        if (item.GetBaseItem().InteractionType == InteractionType.GuildGate)
        {
            item.UpdateCounter = 0;
            item.UpdateNeeded = false;
        }
        RemoveRoomItem(item);
    }

    private void RemoveRoomItem(Item item)
    {
        if (item.IsFloorItem)
            _room.SendPacket(new ObjectRemoveComposer(item, item.UserId));
        else if (item.IsWallItem)
            _room.SendPacket(new ItemRemoveComposer(item, item.UserId));

        //TODO: Recode this specific part
        if (item.IsWallItem)
            _wallItems.TryRemove(item.Id, out item);
        else
        {
            _floorItems.TryRemove(item.Id, out item);
            //mFloorItems.OnCycle();
            _room.GetGameMap().RemoveFromMap(item);
        }
        RemoveItem(item);
        _room.GetGameMap().GenerateMaps();
        _room.GetRoomUserManager().UpdateUserStatusses();
    }

    private List<ServerPacket> CycleRollers()
    {
        if (!GotRollers)
            return new List<ServerPacket>();
        if (_mRollerCycle >= _mRollerSpeed || _mRollerSpeed == 0)
        {
            _rollerItemsMoved.Clear();
            _rollerUsersMoved.Clear();
            _rollerMessages.Clear();
            List<Item> itemsOnRoller;
            List<Item> itemsOnNext;
            foreach (var roller in _rollers.Values.ToList())
            {
                if (roller == null)
                    continue;
                var nextSquare = roller.SquareInFront;
                itemsOnRoller = _room.GetGameMap().GetRoomItemForSquare(roller.GetX, roller.GetY, roller.GetZ);
                itemsOnNext = _room.GetGameMap().GetAllRoomItemForSquare(nextSquare.X, nextSquare.Y).ToList();
                if (itemsOnRoller.Count > 10)
                    itemsOnRoller = _room.GetGameMap().GetRoomItemForSquare(roller.GetX, roller.GetY, roller.GetZ).Take(10).ToList();
                var nextSquareIsRoller = itemsOnNext.Count(x => x.GetBaseItem().InteractionType == InteractionType.Roller) > 0;
                var nextRollerClear = true;
                var nextZ = 0.0;
                var nextRoller = false;
                foreach (var item in itemsOnNext.ToList())
                {
                    if (item.IsRoller)
                    {
                        if (item.TotalHeight > nextZ)
                            nextZ = item.TotalHeight;
                        nextRoller = true;
                    }
                }
                if (nextRoller)
                {
                    foreach (var item in itemsOnNext.ToList())
                    {
                        if (item.TotalHeight > nextZ)
                            nextRollerClear = false;
                    }
                }
                if (itemsOnRoller.Count > 0)
                {
                    foreach (var rItem in itemsOnRoller.ToList())
                    {
                        if (rItem == null)
                            continue;
                        if (!_rollerItemsMoved.Contains(rItem.Id) && _room.GetGameMap().CanRollItemHere(nextSquare.X, nextSquare.Y) && nextRollerClear && roller.GetZ < rItem.GetZ &&
                            _room.GetRoomUserManager().GetUserForSquare(nextSquare.X, nextSquare.Y) == null)
                        {
                            if (!nextSquareIsRoller)
                                nextZ = rItem.GetZ - roller.GetBaseItem().Height;
                            else
                                nextZ = rItem.GetZ;
                            _rollerMessages.Add(UpdateItemOnRoller(rItem, nextSquare, roller.Id, nextZ));
                            _rollerItemsMoved.Add(rItem.Id);
                        }
                    }
                }
                var rollerUser = _room.GetGameMap().GetRoomUsers(roller.Coordinate).FirstOrDefault();
                if (rollerUser != null && !rollerUser.IsWalking && nextRollerClear &&
                    _room.GetGameMap().IsValidStep(new Vector2D(roller.GetX, roller.GetY), new Vector2D(nextSquare.X, nextSquare.Y), true, false, true) &&
                    _room.GetGameMap().CanRollItemHere(nextSquare.X, nextSquare.Y) && _room.GetGameMap().GetFloorStatus(nextSquare) != 0)
                {
                    if (!_rollerUsersMoved.Contains(rollerUser.HabboId))
                    {
                        if (!nextSquareIsRoller)
                            nextZ = rollerUser.Z - roller.GetBaseItem().Height;
                        else
                            nextZ = rollerUser.Z;
                        rollerUser.IsRolling = true;
                        rollerUser.RollerDelay = 1;
                        _rollerMessages.Add(UpdateUserOnRoller(rollerUser, nextSquare, roller.Id, nextZ));
                        _rollerUsersMoved.Add(rollerUser.HabboId);
                    }
                }
            }
            _mRollerCycle = 0;
            return _rollerMessages;
        }
        _mRollerCycle++;
        return new List<ServerPacket>();
    }

    public ServerPacket UpdateItemOnRoller(Item pItem, Point nextCoord, int pRolledId, double nextZ)
    {
        var mMessage = new ServerPacket(ServerPacketHeader.SlideObjectBundleMessageComposer);
        mMessage.WriteInteger(pItem.GetX);
        mMessage.WriteInteger(pItem.GetY);
        mMessage.WriteInteger(nextCoord.X);
        mMessage.WriteInteger(nextCoord.Y);
        mMessage.WriteInteger(1);
        mMessage.WriteInteger(pItem.Id);
        mMessage.WriteString(TextHandling.GetString(pItem.GetZ));
        mMessage.WriteString(TextHandling.GetString(nextZ));
        mMessage.WriteInteger(pRolledId);
        SetFloorItem(pItem, nextCoord.X, nextCoord.Y, nextZ);
        return mMessage;
    }

    public ServerPacket UpdateUserOnRoller(RoomUser pUser, Point pNextCoord, int pRollerId, double nextZ)
    {
        var mMessage = new ServerPacket(ServerPacketHeader.SlideObjectBundleMessageComposer);
        mMessage.WriteInteger(pUser.X);
        mMessage.WriteInteger(pUser.Y);
        mMessage.WriteInteger(pNextCoord.X);
        mMessage.WriteInteger(pNextCoord.Y);
        mMessage.WriteInteger(0);
        mMessage.WriteInteger(pRollerId);
        mMessage.WriteInteger(2);
        mMessage.WriteInteger(pUser.VirtualId);
        mMessage.WriteString(TextHandling.GetString(pUser.Z));
        mMessage.WriteString(TextHandling.GetString(nextZ));
        _room.GetGameMap().UpdateUserMovement(new Point(pUser.X, pUser.Y), new Point(pNextCoord.X, pNextCoord.Y), pUser);
        _room.GetGameMap().GameMap[pUser.X, pUser.Y] = 1;
        pUser.X = pNextCoord.X;
        pUser.Y = pNextCoord.Y;
        pUser.Z = nextZ;
        _room.GetGameMap().GameMap[pUser.X, pUser.Y] = 0;
        if (pUser != null && pUser.GetClient() != null && pUser.GetClient().GetHabbo() != null)
        {
            var items = _room.GetGameMap().GetRoomItemForSquare(pNextCoord.X, pNextCoord.Y);
            foreach (var item in items.ToList())
            {
                if (item == null)
                    continue;
                _room.GetWired().TriggerEvent(WiredBoxType.TriggerWalkOnFurni, pUser.GetClient().GetHabbo(), item);
            }
            var roller = _room.GetRoomItemHandler().GetItem(pRollerId);
            if (roller != null) _room.GetWired().TriggerEvent(WiredBoxType.TriggerWalkOffFurni, pUser.GetClient().GetHabbo(), roller);
        }
        return mMessage;
    }

    private void SaveFurniture()
    {
        try
        {
            if (_movedItems.Count > 0)
            {
                // TODO: Big string builder?
                using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
                foreach (var item in _movedItems.Values.ToList())
                {
                    if (!string.IsNullOrEmpty(item.ExtraData))
                    {
                        dbClient.SetQuery("UPDATE `items` SET `extra_data` = @edata" + item.Id + " WHERE `id` = '" + item.Id + "' LIMIT 1");
                        dbClient.AddParameter("edata" + item.Id, item.ExtraData);
                        dbClient.RunQuery();
                    }
                    if (item.IsWallItem && (!item.GetBaseItem().ItemName.Contains("wallpaper_single") || !item.GetBaseItem().ItemName.Contains("floor_single") ||
                                            !item.GetBaseItem().ItemName.Contains("landscape_single")))
                    {
                        dbClient.SetQuery("UPDATE `items` SET `wall_pos` = @wallPos WHERE `id` = '" + item.Id + "' LIMIT 1");
                        dbClient.AddParameter("wallPos", item.WallCoord);
                        dbClient.RunQuery();
                    }
                    dbClient.RunQuery("UPDATE `items` SET `x` = '" + item.GetX + "', `y` = '" + item.GetY + "', `z` = '" + item.GetZ + "', `rot` = '" + item.Rotation + "' WHERE `id` = '" + item.Id +
                                      "' LIMIT 1");
                }
            }
        }
        catch (Exception e)
        {
            ExceptionLogger.LogCriticalException(e);
        }
    }

    public bool SetFloorItem(GameClient session, Item item, int newX, int newY, int newRot, bool newItem, bool onRoller, bool sendMessage, bool updateRoomUserStatuses = false, double height = -1)
    {
        var needsReAdd = false;
        if (newItem)
        {
            if (item.IsWired)
            {
                if (item.GetBaseItem().WiredType == WiredBoxType.EffectRegenerateMaps &&
                    _room.GetRoomItemHandler().GetFloor.Count(x => x.GetBaseItem().WiredType == WiredBoxType.EffectRegenerateMaps) > 0)
                    return false;
            }
        }
        var itemsOnTile = GetFurniObjects(newX, newY);
        if (item.GetBaseItem().InteractionType == InteractionType.Roller && itemsOnTile.Count(x => x.GetBaseItem().InteractionType == InteractionType.Roller && x.Id != item.Id) > 0)
            return false;
        if (!newItem)
            needsReAdd = _room.GetGameMap().RemoveFromMap(item);
        var affectedTiles = Gamemap.GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, newX, newY, newRot);
        if (!_room.GetGameMap().ValidTile(newX, newY) || _room.GetGameMap().SquareHasUsers(newX, newY) && !item.GetBaseItem().IsSeat)
        {
            if (needsReAdd)
                _room.GetGameMap().AddToMap(item);
            return false;
        }
        foreach (var tile in affectedTiles.Values)
        {
            if (!_room.GetGameMap().ValidTile(tile.X, tile.Y) ||
                _room.GetGameMap().SquareHasUsers(tile.X, tile.Y) && !item.GetBaseItem().IsSeat)
            {
                if (needsReAdd) _room.GetGameMap().AddToMap(item);
                return false;
            }
        }

        // Start calculating new Z coordinate
        double newZ = _room.GetGameMap().Model.SqFloorHeight[newX, newY];
        if (height == -1)
        {
            if (!onRoller)
            {
                // Make sure this tile is open and there are no users here
                if (_room.GetGameMap().Model.SqState[newX, newY] != SquareState.Open && !item.GetBaseItem().IsSeat) return false;
                foreach (var tile in affectedTiles.Values)
                {
                    if (_room.GetGameMap().Model.SqState[tile.X, tile.Y] != SquareState.Open &&
                        !item.GetBaseItem().IsSeat)
                    {
                        if (needsReAdd)
                        {
                            //AddItem(Item);
                            _room.GetGameMap().AddToMap(item);
                        }
                        return false;
                    }
                }

                // And that we have no users
                if (!item.GetBaseItem().IsSeat && !item.IsRoller)
                {
                    foreach (var tile in affectedTiles.Values)
                    {
                        if (_room.GetGameMap().GetRoomUsers(new Point(tile.X, tile.Y)).Count > 0)
                        {
                            if (needsReAdd)
                                _room.GetGameMap().AddToMap(item);
                            return false;
                        }
                    }
                }
            }

            // Find affected objects
            var itemsAffected = new List<Item>();
            var itemsComplete = new List<Item>();
            foreach (var tile in affectedTiles.Values.ToList())
            {
                var temp = GetFurniObjects(tile.X, tile.Y);
                if (temp != null) itemsAffected.AddRange(temp);
            }
            itemsComplete.AddRange(itemsOnTile);
            itemsComplete.AddRange(itemsAffected);
            if (!onRoller)
            {
                // Check for items in the stack that do not allow stacking on top of them
                foreach (var I in itemsComplete.ToList())
                {
                    if (I == null)
                        continue;
                    if (I.Id == item.Id)
                        continue;
                    if (I.GetBaseItem() == null)
                        continue;
                    if (!I.GetBaseItem().Stackable)
                    {
                        if (needsReAdd)
                        {
                            //AddItem(Item);
                            _room.GetGameMap().AddToMap(item);
                        }
                        return false;
                    }
                }
            }

            //if (!Item.IsRoller)
            {
                // If this is a rotating action, maintain item at current height
                if (item.Rotation != newRot && item.GetX == newX && item.GetY == newY)
                    newZ = item.GetZ;

                // Are there any higher objects in the stack!?
                foreach (var i in itemsComplete.ToList())
                {
                    if (i == null)
                        continue;
                    if (i.Id == item.Id)
                        continue;
                    if (i.GetBaseItem().InteractionType == InteractionType.Stacktool)
                    {
                        newZ = i.GetZ;
                        break;
                    }
                    if (i.TotalHeight > newZ) newZ = i.TotalHeight;
                }
            }

            // Verify the rotation is correct
            if (newRot != 0 && newRot != 2 && newRot != 4 && newRot != 6 && newRot != 8 && !item.GetBaseItem().ExtraRot)
                newRot = 0;
        }
        else
            newZ = height;
        item.Rotation = newRot;
        var oldX = item.GetX;
        var oldY = item.GetY;
        item.SetState(newX, newY, newZ, affectedTiles);
        if (!onRoller && session != null)
            item.Interactor.OnPlace(session, item);
        if (newItem)
        {
            if (_floorItems.ContainsKey(item.Id))
            {
                if (session != null)
                    session.SendNotification(PlusEnvironment.GetLanguageManager().TryGetValue("room.item.already_placed"));
                _room.GetGameMap().RemoveFromMap(item);
                return true;
            }
            if (item.IsFloorItem && !_floorItems.ContainsKey(item.Id))
                _floorItems.TryAdd(item.Id, item);
            else if (item.IsWallItem && !_wallItems.ContainsKey(item.Id))
                _wallItems.TryAdd(item.Id, item);
            if (sendMessage)
                _room.SendPacket(new ObjectAddComposer(item));
        }
        else
        {
            UpdateItem(item);
            if (!onRoller && sendMessage)
                _room.SendPacket(new ObjectUpdateComposer(item, _room.OwnerId));
        }
        _room.GetGameMap().AddToMap(item);
        if (item.GetBaseItem().IsSeat)
            updateRoomUserStatuses = true;
        if (updateRoomUserStatuses)
            _room.GetRoomUserManager().UpdateUserStatusses();
        if (item.GetBaseItem().InteractionType == InteractionType.Tent || item.GetBaseItem().InteractionType == InteractionType.TentSmall)
        {
            _room.RemoveTent(item.Id);
            _room.AddTent(item.Id);
        }
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.RunQuery("UPDATE `items` SET `room_id` = '" + _room.RoomId + "', `x` = '" + item.GetX + "', `y` = '" + item.GetY + "', `z` = '" + item.GetZ + "', `rot` = '" + item.Rotation +
                          "' WHERE `id` = '" + item.Id + "' LIMIT 1");
        return true;
    }


    public List<Item> GetFurniObjects(int x, int y) => _room.GetGameMap().GetCoordinatedItems(new Point(x, y));

    public bool SetFloorItem(Item item, int newX, int newY, double newZ)
    {
        if (_room == null)
            return false;
        _room.GetGameMap().RemoveFromMap(item);
        item.SetState(newX, newY, newZ, Gamemap.GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, newX, newY, item.Rotation));
        if (item.GetBaseItem().InteractionType == InteractionType.Toner)
            if (_room.TonerData == null)
                _room.TonerData = new TonerData(item.Id);
        UpdateItem(item);
        _room.GetGameMap().AddItemToMap(item);
        return true;
    }

    public bool SetWallItem(GameClient session, Item item)
    {
        if (!item.IsWallItem || _wallItems.ContainsKey(item.Id))
            return false;
        if (_floorItems.ContainsKey(item.Id))
        {
            session.SendNotification(PlusEnvironment.GetLanguageManager().TryGetValue("room.item.already_placed"));
            return true;
        }
        item.Interactor.OnPlace(session, item);
        if (item.GetBaseItem().InteractionType == InteractionType.Moodlight)
        {
            if (_room.MoodlightData == null)
            {
                _room.MoodlightData = new MoodlightData(item.Id);
                item.ExtraData = _room.MoodlightData.GenerateExtraData();
            }
        }
        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            dbClient.SetQuery("UPDATE `items` SET `room_id` = '" + _room.RoomId + "', `x` = '" + item.GetX + "', `y` = '" + item.GetY + "', `z` = '" + item.GetZ + "', `rot` = '" + item.Rotation +
                              "', `wall_pos` = @WallPos WHERE `id` = '" + item.Id + "' LIMIT 1");
            dbClient.AddParameter("WallPos", item.WallCoord);
            dbClient.RunQuery();
        }
        _wallItems.TryAdd(item.Id, item);
        _room.SendPacket(new ItemAddComposer(item));
        return true;
    }

    public void UpdateItem(Item item)
    {
        if (item == null)
            return;
        if (!_movedItems.ContainsKey(item.Id))
            _movedItems.TryAdd(item.Id, item);
    }


    public void RemoveItem(Item item)
    {
        if (item == null)
            return;
        if (_movedItems.ContainsKey(item.Id))
            _movedItems.TryRemove(item.Id, out item);
        if (_rollers.ContainsKey(item.Id))
            _rollers.TryRemove(item.Id, out item);
    }

    public void OnCycle()
    {
        if (GotRollers)
        {
            try
            {
                _room.SendPacket(CycleRollers());
            }
            catch //(Exception e)
            {
                // Logging.LogThreadException(e.ToString(), "rollers for room with ID " + room.RoomId);
                GotRollers = false;
            }
        }
        if (_roomItemUpdateQueue.Count > 0)
        {
            var addItems = new List<Item>();
            while (_roomItemUpdateQueue.Count > 0)
            {
                var item = (Item)null;
                if (_roomItemUpdateQueue.TryDequeue(out item))
                {
                    item.ProcessUpdates();
                    if (item.UpdateCounter > 0)
                        addItems.Add(item);
                }
            }
            foreach (var item in addItems.ToList())
            {
                if (item == null)
                    continue;
                _roomItemUpdateQueue.Enqueue(item);
            }
        }
    }

    public List<Item> RemoveItems(GameClient session)
    {
        var items = new List<Item>();
        foreach (var item in GetWallAndFloor.ToList())
        {
            if (item == null || item.UserId != session.GetHabbo().Id)
                continue;
            if (item.IsFloorItem)
            {
                _floorItems.TryRemove(item.Id, out var I);
                session.GetHabbo().Inventory.Furniture.AddItem(I);
                _room.SendPacket(new ObjectRemoveComposer(item, item.UserId));
            }
            else if (item.IsWallItem)
            {
                _wallItems.TryRemove(item.Id, out var I);
                session.GetHabbo().Inventory.Furniture.AddItem(I);
                _room.SendPacket(new ItemRemoveComposer(item, item.UserId));
            }
            session.SendPacket(new FurniListAddComposer(item));
        }
        _rollers.Clear();
        return items;
    }


    public bool CheckPosItem(Item item, int newX, int newY, int newRot)
    {
        try
        {
            var dictionary = Gamemap.GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, newX, newY, newRot);
            if (!_room.GetGameMap().ValidTile(newX, newY))
                return false;
            foreach (var coord in dictionary.Values.ToList())
            {
                if (_room.GetGameMap().Model.DoorX == coord.X && _room.GetGameMap().Model.DoorY == coord.Y)
                    return false;
            }
            if (_room.GetGameMap().Model.DoorX == newX && _room.GetGameMap().Model.DoorY == newY)
                return false;
            foreach (var coord in dictionary.Values.ToList())
            {
                if (!_room.GetGameMap().ValidTile(coord.X, coord.Y))
                    return false;
            }
            double num = _room.GetGameMap().Model.SqFloorHeight[newX, newY];
            if (item.Rotation == newRot && item.GetX == newX && item.GetY == newY && item.GetZ != num)
                return false;
            if (_room.GetGameMap().Model.SqState[newX, newY] != SquareState.Open)
                return false;
            foreach (var coord in dictionary.Values.ToList())
            {
                if (_room.GetGameMap().Model.SqState[coord.X, coord.Y] != SquareState.Open)
                    return false;
            }
            if (!item.GetBaseItem().IsSeat)
            {
                if (_room.GetGameMap().SquareHasUsers(newX, newY))
                    return false;
                foreach (var coord in dictionary.Values.ToList())
                {
                    if (_room.GetGameMap().SquareHasUsers(coord.X, coord.Y))
                        return false;
                }
            }
            var furniObjects = GetFurniObjects(newX, newY);
            var collection = new List<Item>();
            var list3 = new List<Item>();
            foreach (var coord in dictionary.Values.ToList())
            {
                var list4 = GetFurniObjects(coord.X, coord.Y);
                if (list4 != null)
                    collection.AddRange(list4);
            }
            if (furniObjects == null)
                furniObjects = new List<Item>();
            list3.AddRange(furniObjects);
            list3.AddRange(collection);
            foreach (var i in list3.ToList())
            {
                if (i.Id != item.Id && !i.GetBaseItem().Stackable)
                    return false;
            }
            return true;
        }
        catch
        {
            return false;
        }
    }


    public ICollection<Item> GetRollers() => _rollers.Values;

    public void Dispose()
    {
        SaveFurniture();
        foreach (var item in GetWallAndFloor.ToList())
        {
            if (item == null)
                continue;
            item.Destroy();
        }
        _movedItems.Clear();
        _rollers.Clear();
        _wallItems.Clear();
        _floorItems.Clear();
        _rollerItemsMoved.Clear();
        _rollerUsersMoved.Clear();
        _rollerMessages.Clear();
        _roomItemUpdateQueue = null;
    }
}
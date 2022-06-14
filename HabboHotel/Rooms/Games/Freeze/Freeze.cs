using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Rooms.Freeze;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Rooms.Games.Teams;
using System.Collections.Concurrent;
using System.Drawing;

namespace Plus.HabboHotel.Rooms.Games.Freeze;

public class Freeze
{
    private readonly ConcurrentDictionary<int, Item> _freezeBlocks;
    private readonly ConcurrentDictionary<int, Item> _freezeTiles;
    private Room _room;

    public Freeze(Room room)
    {
        _room = room;
        GameIsStarted = false;
        ExitTeleports = new ConcurrentDictionary<int, Item>();
        _freezeTiles = new ConcurrentDictionary<int, Item>();
        _freezeBlocks = new ConcurrentDictionary<int, Item>();
    }

    public bool GameIsStarted { get; private set; }

    public ConcurrentDictionary<int, Item> ExitTeleports { get; }

    public void AddExitTile(Item item)
    {
        if (!ExitTeleports.ContainsKey(item.Id))
            ExitTeleports.TryAdd(item.Id, item);
    }

    public void RemoveExitTile(int id)
    {
        Item temp;
        if (ExitTeleports.ContainsKey(id))
            ExitTeleports.TryRemove(id, out temp);
    }

    public Item GetRandomExitTile() => ExitTeleports.Values.ToList()[Random.Shared.Next(0, ExitTeleports.Count)];

    public void StartGame()
    {
        GameIsStarted = true;
        CountTeamPoints();
        ResetGame();
        if (ExitTeleports.Count > 0)
        {
            foreach (var exitTile in ExitTeleports.Values.ToList())
            {
                if (exitTile.ExtraData == "0" || string.IsNullOrEmpty(exitTile.ExtraData))
                    exitTile.ExtraData = "1";
                exitTile.UpdateState();
            }
        }
        _room.GetGameManager().LockGates();
    }

    public void StopGame(bool userTriggered = false)
    {
        GameIsStarted = false;
        _room.GetGameManager().UnlockGates();
        _room.GetGameManager().StopGame();
        ResetGame();
        if (ExitTeleports.Count > 0)
        {
            foreach (var exitTile in ExitTeleports.Values.ToList())
            {
                if (exitTile.ExtraData == "1" || string.IsNullOrEmpty(exitTile.ExtraData))
                    exitTile.ExtraData = "0";
                exitTile.UpdateState();
            }
        }
        var winners = _room.GetGameManager().GetWinningTeam();
        foreach (var user in _room.GetRoomUserManager().GetUserList().ToList())
        {
            user.FreezeLives = 0;
            if (user.Team == winners)
            {
                user.UnIdle();
                user.DanceId = 0;
                _room.SendPacket(new ActionComposer(user.VirtualId, 1));
            }
            if (ExitTeleports.Count > 0)
            {
                var tile = _freezeTiles.Values.Where(x => x.GetX == user.X && x.GetY == user.Y).FirstOrDefault();
                if (tile != null)
                {
                    var exitTle = GetRandomExitTile();
                    if (exitTle != null)
                    {
                        _room.GetGameMap().UpdateUserMovement(user.Coordinate, exitTle.Coordinate, user);
                        user.SetPos(exitTle.GetX, exitTle.GetY, exitTle.GetZ);
                        user.UpdateNeeded = true;
                        if (user.IsAsleep)
                            user.UnIdle();
                    }
                }
            }
        }
        if (!userTriggered)
            _room.GetWired().TriggerEvent(WiredBoxType.TriggerGameEnds, null);
    }

    public void CycleUser(RoomUser user)
    {
        if (user.Freezed)
        {
            user.FreezeCounter++;
            if (user.FreezeCounter > 10)
            {
                user.Freezed = false;
                user.FreezeCounter = 0;
                ActivateShield(user);
            }
        }
        if (user.ShieldActive)
        {
            user.ShieldCounter++;
            if (user.ShieldCounter > 10)
            {
                user.ShieldActive = false;
                user.ShieldCounter = 10;
                user.ApplyEffect(Convert.ToInt32(user.Team) + 39);
            }
        }
    }

    public void ResetGame()
    {
        foreach (var item in _freezeTiles.Values.ToList())
        {
            if (!string.IsNullOrEmpty(item.ExtraData))
            {
                item.InteractionCountHelper = 0;
                item.ExtraData = "";
                item.UpdateState(false, true);
                _room.GetGameMap().AddItemToMap(item, false);
            }
        }
        foreach (var item in _freezeBlocks.Values)
        {
            if (!string.IsNullOrEmpty(item.ExtraData))
            {
                item.ExtraData = "";
                item.UpdateState(false, true);
                _room.GetGameMap().AddItemToMap(item, false);
            }
        }
    }

    public void OnUserWalk(RoomUser user)
    {
        if (!GameIsStarted || user.Team == Team.None)
            return;
        foreach (var item in _freezeTiles.Values.ToList())
        {
            if (user.GoalX == item.GetX && user.GoalY == item.GetY && user.FreezeInteracting)
            {
                if (item.InteractionCountHelper == 0)
                {
                    item.InteractionCountHelper = 1;
                    item.ExtraData = "1000";
                    item.UpdateState();
                    item.InteractingUser = user.UserId;
                    item.FreezePowerUp = user.BanzaiPowerUp;
                    item.RequestUpdate(4, true);
                    switch (user.BanzaiPowerUp)
                    {
                        case FreezePowerUp.GreenArrow:
                        case FreezePowerUp.OrangeSnowball:
                            {
                                user.BanzaiPowerUp = FreezePowerUp.None;
                                break;
                            }
                    }
                    break;
                }
            }
        }
        foreach (var item in _freezeBlocks.Values.ToList())
        {
            if (user.GoalX == item.GetX && user.GoalY == item.GetY)
                if (item.FreezePowerUp != FreezePowerUp.None)
                    PickUpPowerUp(item, user);
        }
    }

    private void CountTeamPoints()
    {
        _room.GetGameManager().Reset();
        foreach (var user in _room.GetRoomUserManager().GetUserList().ToList())
        {
            if (user.IsBot || user.Team == Team.None || user.GetClient() == null)
                continue;
            user.BanzaiPowerUp = FreezePowerUp.None;
            user.FreezeLives = 3;
            user.ShieldActive = false;
            user.ShieldCounter = 11;
            _room.GetGameManager().AddPointToTeam(user.Team, 30);
            user.GetClient().SendPacket(new UpdateFreezeLivesComposer(user.InternalRoomId, user.FreezeLives));
        }
    }

    public void OnFreezeTiles(Item item, FreezePowerUp powerUp)
    {
        List<Item> items;
        switch (powerUp)
        {
            case FreezePowerUp.BlueArrow:
                {
                    items = GetVerticalItems(item.GetX, item.GetY, 5);
                    break;
                }
            case FreezePowerUp.GreenArrow:
                {
                    items = GetDiagonalItems(item.GetX, item.GetY, 5);
                    break;
                }
            case FreezePowerUp.OrangeSnowball:
                {
                    items = GetVerticalItems(item.GetX, item.GetY, 5);
                    items.AddRange(GetDiagonalItems(item.GetX, item.GetY, 5));
                    break;
                }
            default:
                {
                    items = GetVerticalItems(item.GetX, item.GetY, 3);
                    break;
                }
        }
        HandleBanzaiFreezeItems(items);
    }

    private static void ActivateShield(RoomUser user)
    {
        user.ApplyEffect(Convert.ToInt32(user.Team + 48));
        user.ShieldActive = true;
        user.ShieldCounter = 0;
    }

    private void HandleBanzaiFreezeItems(List<Item> items)
    {
        foreach (var item in items.ToList())
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.FreezeTile:
                    {
                        item.ExtraData = "11000";
                        item.UpdateState(false, true);
                        continue;
                    }
                case InteractionType.FreezeTileBlock:
                    {
                        SetRandomPowerUp(item);
                        item.UpdateState(false, true);
                        continue;
                    }
                default:
                    {
                        continue;
                    }
            }
        }
    }

    private void SetRandomPowerUp(Item item)
    {
        if (!string.IsNullOrEmpty(item.ExtraData))
            return;
        var next = Random.Shared.Next(1, 14);
        switch (next)
        {
            case 2:
                {
                    item.ExtraData = "2000";
                    item.FreezePowerUp = FreezePowerUp.BlueArrow;
                    break;
                }
            case 3:
                {
                    item.ExtraData = "3000";
                    item.FreezePowerUp = FreezePowerUp.Snowballs;
                    break;
                }
            case 4:
                {
                    item.ExtraData = "4000";
                    item.FreezePowerUp = FreezePowerUp.GreenArrow;
                    break;
                }
            case 5:
                {
                    item.ExtraData = "5000";
                    item.FreezePowerUp = FreezePowerUp.OrangeSnowball;
                    break;
                }
            case 6:
                {
                    item.ExtraData = "6000";
                    item.FreezePowerUp = FreezePowerUp.Heart;
                    break;
                }
            case 7:
                {
                    item.ExtraData = "7000";
                    item.FreezePowerUp = FreezePowerUp.Shield;
                    break;
                }
            default:
                {
                    item.ExtraData = "1000";
                    item.FreezePowerUp = FreezePowerUp.None;
                    break;
                }
        }
        _room.GetGameMap().RemoveFromMap(item, false);
        item.UpdateState(false, true);
    }

    private void PickUpPowerUp(Item item, RoomUser user)
    {
        switch (item.FreezePowerUp)
        {
            case FreezePowerUp.Heart:
                {
                    if (user.FreezeLives < 5)
                    {
                        user.FreezeLives++;
                        _room.GetGameManager().AddPointToTeam(user.Team, 10);
                    }
                    user.GetClient().SendPacket(new UpdateFreezeLivesComposer(user.InternalRoomId, user.FreezeLives));
                    break;
                }
            case FreezePowerUp.Shield:
                {
                    ActivateShield(user);
                    break;
                }
            case FreezePowerUp.BlueArrow:
            case FreezePowerUp.GreenArrow:
            case FreezePowerUp.OrangeSnowball:
                {
                    user.BanzaiPowerUp = item.FreezePowerUp;
                    break;
                }
        }
        item.FreezePowerUp = FreezePowerUp.None;
        item.ExtraData = "1" + item.ExtraData;
        item.UpdateState(false, true);
    }

    public void AddFreezeTile(Item item)
    {
        if (!_freezeTiles.ContainsKey(item.Id))
            _freezeTiles.TryAdd(item.Id, item);
    }

    public void RemoveFreezeTile(int itemId)
    {
        Item item = null;
        if (_freezeTiles.ContainsKey(itemId))
            _freezeTiles.TryRemove(itemId, out item);
    }

    public void AddFreezeBlock(Item item)
    {
        if (!_freezeBlocks.ContainsKey(item.Id))
            _freezeBlocks.TryAdd(item.Id, item);
    }

    public void RemoveFreezeBlock(int itemId)
    {
        Item item = null;
        _freezeBlocks.TryRemove(itemId, out item);
    }

    private void HandleUserFreeze(Point point)
    {
        if (_room == null)
            return;
        var user = _room.GetGameMap().GetRoomUsers(point).FirstOrDefault();
        if (user != null)
        {
            if (user.IsWalking && user.SetX != point.X && user.SetY != point.Y)
                return;
            FreezeUser(user);
        }
    }

    private void FreezeUser(RoomUser user)
    {
        if (user.IsBot || user.ShieldActive || user.Team == Team.None || user.Freezed)
            return;
        user.Freezed = true;
        user.FreezeCounter = 0;
        user.FreezeLives--;
        if (user.FreezeLives <= 0)
        {
            user.GetClient().SendPacket(new UpdateFreezeLivesComposer(user.InternalRoomId, user.FreezeLives));
            user.ApplyEffect(-1);
            _room.GetGameManager().AddPointToTeam(user.Team, -10);
            var t = _room.GetTeamManagerForFreeze();
            t.OnUserLeave(user);
            user.Team = Team.None;
            if (ExitTeleports.Count > 0)
                _room.GetGameMap().TeleportToItem(user, GetRandomExitTile());
            user.Freezed = false;
            user.SetStep = false;
            user.IsWalking = false;
            user.UpdateNeeded = true;
            if (t.BlueTeam.Count <= 0 && t.RedTeam.Count <= 0 && t.GreenTeam.Count <= 0 && t.YellowTeam.Count > 0)
                StopGame(); // yellow team win
            else if (t.BlueTeam.Count > 0 && t.RedTeam.Count <= 0 && t.GreenTeam.Count <= 0 &&
                     t.YellowTeam.Count <= 0)
                StopGame(); // blue team win
            else if (t.BlueTeam.Count <= 0 && t.RedTeam.Count > 0 && t.GreenTeam.Count <= 0 &&
                     t.YellowTeam.Count <= 0)
                StopGame(); // red team win
            else if (t.BlueTeam.Count <= 0 && t.RedTeam.Count <= 0 && t.GreenTeam.Count > 0 &&
                     t.YellowTeam.Count <= 0)
                StopGame(); // green team win
            return;
        }
        _room.GetGameManager().AddPointToTeam(user.Team, -10);
        user.ApplyEffect(12);
        user.GetClient().SendPacket(new UpdateFreezeLivesComposer(user.InternalRoomId, user.FreezeLives));
    }

    private List<Item> GetVerticalItems(int x, int y, int length)
    {
        var totalItems = new List<Item>();
        for (var i = 0; i < length; i++)
        {
            var point = new Point(x + i, y);
            var items = GetItemsForSquare(point);
            if (!SquareGotFreezeTile(items))
                break;
            HandleUserFreeze(point);
            totalItems.AddRange(items);
            if (SquareGotFreezeBlock(items))
                break;
        }
        for (var i = 1; i < length; i++)
        {
            var point = new Point(x, y + i);
            var items = GetItemsForSquare(point);
            if (!SquareGotFreezeTile(items))
                break;
            HandleUserFreeze(point);
            totalItems.AddRange(items);
            if (SquareGotFreezeBlock(items))
                break;
        }
        for (var i = 1; i < length; i++)
        {
            var point = new Point(x - i, y);
            var items = GetItemsForSquare(point);
            if (!SquareGotFreezeTile(items))
                break;
            HandleUserFreeze(point);
            totalItems.AddRange(items);
            if (SquareGotFreezeBlock(items))
                break;
        }
        for (var i = 1; i < length; i++)
        {
            var point = new Point(x, y - i);
            var items = GetItemsForSquare(point);
            if (!SquareGotFreezeTile(items))
                break;
            HandleUserFreeze(point);
            totalItems.AddRange(items);
            if (SquareGotFreezeBlock(items))
                break;
        }
        return totalItems;
    }

    private List<Item> GetDiagonalItems(int x, int y, int length)
    {
        var totalItems = new List<Item>();
        for (var i = 0; i < length; i++)
        {
            var point = new Point(x + i, y + i);
            var items = GetItemsForSquare(point);
            if (!SquareGotFreezeTile(items))
                break;
            HandleUserFreeze(point);
            totalItems.AddRange(items);
            if (SquareGotFreezeBlock(items))
                break;
        }
        for (var i = 0; i < length; i++)
        {
            var point = new Point(x - i, y - i);
            var items = GetItemsForSquare(point);
            if (!SquareGotFreezeTile(items))
                break;
            HandleUserFreeze(point);
            totalItems.AddRange(items);
            if (SquareGotFreezeBlock(items))
                break;
        }
        for (var i = 0; i < length; i++)
        {
            var point = new Point(x - i, y + i);
            var items = GetItemsForSquare(point);
            if (!SquareGotFreezeTile(items))
                break;
            HandleUserFreeze(point);
            totalItems.AddRange(items);
            if (SquareGotFreezeBlock(items))
                break;
        }
        for (var i = 0; i < length; i++)
        {
            var point = new Point(x + i, y - i);
            var items = GetItemsForSquare(point);
            if (!SquareGotFreezeTile(items))
                break;
            HandleUserFreeze(point);
            totalItems.AddRange(items);
            if (SquareGotFreezeBlock(items))
                break;
        }
        return totalItems;
    }

    private List<Item> GetItemsForSquare(Point point) => _room.GetGameMap().GetCoordinatedItems(point);

    private static bool SquareGotFreezeTile(List<Item> items)
    {
        foreach (var item in items)
        {
            if (item.GetBaseItem().InteractionType == InteractionType.FreezeTile)
                return true;
        }
        return false;
    }

    private static bool SquareGotFreezeBlock(List<Item> items)
    {
        foreach (var item in items)
        {
            if (item.GetBaseItem().InteractionType == InteractionType.FreezeTileBlock)
                return true;
        }
        return false;
    }

    public void Dispose()
    {
        _room = null;
        ExitTeleports.Clear();
        _freezeTiles.Clear();
        _freezeBlocks.Clear();
    }
}
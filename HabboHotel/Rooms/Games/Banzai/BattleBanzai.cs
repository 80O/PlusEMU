using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Rooms.Games.Teams;
using Plus.HabboHotel.Rooms.PathFinding;
using Plus.Utilities;
using Plus.Utilities.Enclosure;
using System.Collections.Concurrent;
using System.Drawing;

namespace Plus.HabboHotel.Rooms.Games.Banzai;

public class BattleBanzai
{
    private ConcurrentDictionary<int, Item> _banzaiTiles;
    private GameField _field;
    private byte[,] _floorMap;
    private ConcurrentDictionary<int, Item> _pucks;
    private Room _room;
    private double _timestarted;

    public BattleBanzai(Room room)
    {
        _room = room;
        IsBanzaiActive = false;
        _timestarted = 0;
        _pucks = new ConcurrentDictionary<int, Item>();
        _banzaiTiles = new ConcurrentDictionary<int, Item>();
    }

    public bool IsBanzaiActive { get; private set; }

    public void AddTile(Item item, int itemId)
    {
        if (!_banzaiTiles.ContainsKey(itemId))
            _banzaiTiles.TryAdd(itemId, item);
    }

    public void RemoveTile(int itemId)
    {
        _banzaiTiles.TryRemove(itemId, out var item);
    }

    public void AddPuck(Item item)
    {
        if (!_pucks.ContainsKey(item.Id))
            _pucks.TryAdd(item.Id, item);
    }

    public void RemovePuck(int itemId)
    {
        _pucks.TryRemove(itemId, out var item);
    }

    public void OnUserWalk(RoomUser user)
    {
        if (user == null)
            return;
        foreach (var item in _pucks.Values.ToList())
        {
            var newX = 0;
            var newY = 0;
            var differenceX = user.X - item.GetX;
            var differenceY = user.Y - item.GetY;
            if (differenceX == 0 && differenceY == 0)
            {
                if (user.RotBody == 4)
                {
                    newX = user.X;
                    newY = user.Y + 2;
                }
                else if (user.RotBody == 6)
                {
                    newX = user.X - 2;
                    newY = user.Y;
                }
                else if (user.RotBody == 0)
                {
                    newX = user.X;
                    newY = user.Y - 2;
                }
                else if (user.RotBody == 2)
                {
                    newX = user.X + 2;
                    newY = user.Y;
                }
                else if (user.RotBody == 1)
                {
                    newX = user.X + 2;
                    newY = user.Y - 2;
                }
                else if (user.RotBody == 7)
                {
                    newX = user.X - 2;
                    newY = user.Y - 2;
                }
                else if (user.RotBody == 3)
                {
                    newX = user.X + 2;
                    newY = user.Y + 2;
                }
                else if (user.RotBody == 5)
                {
                    newX = user.X - 2;
                    newY = user.Y + 2;
                }
                if (!_room.GetRoomItemHandler().CheckPosItem(item, newX, newY, item.Rotation))
                {
                    if (user.RotBody == 0)
                    {
                        newX = user.X;
                        newY = user.Y + 1;
                    }
                    else if (user.RotBody == 2)
                    {
                        newX = user.X - 1;
                        newY = user.Y;
                    }
                    else if (user.RotBody == 4)
                    {
                        newX = user.X;
                        newY = user.Y - 1;
                    }
                    else if (user.RotBody == 6)
                    {
                        newX = user.X + 1;
                        newY = user.Y;
                    }
                    else if (user.RotBody == 5)
                    {
                        newX = user.X + 1;
                        newY = user.Y - 1;
                    }
                    else if (user.RotBody == 3)
                    {
                        newX = user.X - 1;
                        newY = user.Y - 1;
                    }
                    else if (user.RotBody == 7)
                    {
                        newX = user.X + 1;
                        newY = user.Y + 1;
                    }
                    else if (user.RotBody == 1)
                    {
                        newX = user.X - 1;
                        newY = user.Y + 1;
                    }
                }
            }
            else if (differenceX <= 1 && differenceX >= -1 && differenceY <= 1 && differenceY >= -1 &&
                     VerifyPuck(user, item.Coordinate.X, item.Coordinate.Y)) //VERYFIC BALL CHECAR SI ESTA EN DIRECCION ASIA LA PELOTA
            {
                newX = differenceX * -1;
                newY = differenceY * -1;
                newX = newX + item.GetX;
                newY = newY + item.GetY;
            }
            if (item.GetRoom().GetGameMap().ValidTile(newX, newY)) MovePuck(item, user.GetClient(), newX, newY, user.Team);
        }
        if (IsBanzaiActive) HandleBanzaiTiles(user.Coordinate, user.Team, user);
    }

    private bool VerifyPuck(RoomUser user, int actualx, int actualy) => Rotation.Calculate(user.X, user.Y, actualx, actualy) == user.RotBody;

    public void BanzaiStart()
    {
        if (IsBanzaiActive)
            return;
        _floorMap = new byte[_room.GetGameMap().Model.MapSizeY, _room.GetGameMap().Model.MapSizeX];
        _field = new GameField(_floorMap, true);
        _timestarted = UnixTimestamp.GetNow();
        _room.GetGameManager().LockGates();
        for (var i = 1; i < 5; i++) _room.GetGameManager().Points[i] = 0;
        foreach (var tile in _banzaiTiles.Values)
        {
            tile.ExtraData = "1";
            tile.Value = 0;
            tile.Team = Team.None;
            tile.UpdateState();
        }
        ResetTiles();
        IsBanzaiActive = true;
        _room.GetWired().TriggerEvent(WiredBoxType.TriggerGameStarts, null);
        foreach (var user in _room.GetRoomUserManager().GetRoomUsers()) user.LockedTilesCount = 0;
    }

    public void ResetTiles()
    {
        foreach (var item in _room.GetRoomItemHandler().GetFloor.ToList())
        {
            var type = item.GetBaseItem().InteractionType;
            switch (type)
            {
                case InteractionType.Banzaiscoreblue:
                case InteractionType.Banzaiscoregreen:
                case InteractionType.Banzaiscorered:
                case InteractionType.Banzaiscoreyellow:
                    {
                        item.ExtraData = "0";
                        item.UpdateState();
                        break;
                    }
            }
        }
    }

    public void BanzaiEnd(bool triggeredByUser = false)
    {
        IsBanzaiActive = false;
        _room.GetGameManager().StopGame();
        _floorMap = null;
        if (!triggeredByUser)
            _room.GetWired().TriggerEvent(WiredBoxType.TriggerGameEnds, null);
        var winners = _room.GetGameManager().GetWinningTeam();
        _room.GetGameManager().UnlockGates();
        foreach (var tile in _banzaiTiles.Values)
        {
            if (tile.Team == winners)
            {
                tile.InteractionCount = 0;
                tile.InteractionCountHelper = 0;
                tile.UpdateNeeded = true;
            }
            else if (tile.Team == Team.None)
            {
                tile.ExtraData = "0";
                tile.UpdateState();
            }
        }
        if (winners != Team.None)
        {
            foreach (var user in _room.GetRoomUserManager().GetRoomUsers().ToList())
            {
                if (user.Team != Team.None)
                {
                    if (UnixTimestamp.GetNow() - _timestarted > 5)
                    {
                        PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(user.GetClient(), "ACH_BattleBallTilesLocked", user.LockedTilesCount);
                        PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(user.GetClient(), "ACH_BattleBallPlayer", 1);
                    }
                }
                if (winners == Team.Blue)
                {
                    if (user.CurrentEffect == 35)
                    {
                        if (UnixTimestamp.GetNow() - _timestarted > 5)
                            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(user.GetClient(), "ACH_BattleBallWinner", 1);
                        _room.SendPacket(new ActionComposer(user.VirtualId, 1));
                    }
                }
                else if (winners == Team.Red)
                {
                    if (user.CurrentEffect == 33)
                    {
                        if (UnixTimestamp.GetNow() - _timestarted > 5)
                            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(user.GetClient(), "ACH_BattleBallWinner", 1);
                        _room.SendPacket(new ActionComposer(user.VirtualId, 1));
                    }
                }
                else if (winners == Team.Green)
                {
                    if (user.CurrentEffect == 34)
                    {
                        if (UnixTimestamp.GetNow() - _timestarted > 5)
                            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(user.GetClient(), "ACH_BattleBallWinner", 1);
                        _room.SendPacket(new ActionComposer(user.VirtualId, 1));
                    }
                }
                else if (winners == Team.Yellow)
                {
                    if (user.CurrentEffect == 36)
                    {
                        if (UnixTimestamp.GetNow() - _timestarted > 5)
                            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(user.GetClient(), "ACH_BattleBallWinner", 1);
                        _room.SendPacket(new ActionComposer(user.VirtualId, 1));
                    }
                }
            }
            if (_field != null)
                _field.Dispose();
        }
    }

    public void MovePuck(Item item, GameClient mover, int newX, int newY, Team team)
    {
        if (!_room.GetGameMap().ItemCanBePlaced(newX, newY))
            return;
        var oldRoomCoord = item.Coordinate;
        if (oldRoomCoord.X == newX && oldRoomCoord.Y == newY)
            return;
        item.ExtraData = Convert.ToInt32(team).ToString();
        item.UpdateNeeded = true;
        item.UpdateState();
        double newZ = _room.GetGameMap().Model.SqFloorHeight[newX, newY];
        _room.SendPacket(new SlideObjectBundleComposer(item.GetX, item.GetY, item.GetZ, newX, newY, newZ, 0, 0, item.Id));
        _room.GetRoomItemHandler().SetFloorItem(mover, item, newX, newY, item.Rotation, false, false, false);
        if (mover == null || mover.GetHabbo() == null)
            return;
        var user = mover.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(mover.GetHabbo().Id);
        if (IsBanzaiActive) HandleBanzaiTiles(new Point(newX, newY), team, user);
    }

    private void SetTile(Item item, Team team, RoomUser user)
    {
        if (item.Team == team)
        {
            if (item.Value < 3)
            {
                item.Value++;
                if (item.Value == 3)
                {
                    user.LockedTilesCount++;
                    _room.GetGameManager().AddPointToTeam(item.Team, 1);
                    _field.UpdateLocation(item.GetX, item.GetY, (byte)team);
                    var gfield = _field.DoUpdate();
                    Team t;
                    foreach (var gameField in gfield)
                    {
                        t = (Team)gameField.ForValue;
                        foreach (var p in gameField.GetPoints())
                        {
                            HandleMaxBanzaiTiles(new Point(p.X, p.Y), t);
                            _floorMap[p.Y, p.X] = gameField.ForValue;
                        }
                    }
                }
            }
        }
        else
        {
            if (item.Value < 3)
            {
                item.Team = team;
                item.Value = 1;
            }
        }
        var newColor = item.Value + Convert.ToInt32(item.Team) * 3 - 1;
        item.ExtraData = newColor.ToString();
    }

    private void HandleBanzaiTiles(Point coord, Team team, RoomUser user)
    {
        if (team == Team.None)
            return;
        var items = _room.GetGameMap().GetCoordinatedItems(coord);
        var i = 0;
        foreach (var item in _banzaiTiles.Values.ToList())
        {
            if (item == null)
                continue;
            if (item.GetBaseItem().InteractionType != InteractionType.Banzaifloor)
            {
                user.Team = Team.None;
                user.ApplyEffect(0);
                continue;
            }
            if (item.ExtraData.Equals("5") || item.ExtraData.Equals("8") || item.ExtraData.Equals("11") ||
                item.ExtraData.Equals("14"))
            {
                i++;
                continue;
            }
            if (item.GetX != coord.X || item.GetY != coord.Y)
                continue;
            SetTile(item, team, user);
            if (item.ExtraData.Equals("5") || item.ExtraData.Equals("8") || item.ExtraData.Equals("11") ||
                item.ExtraData.Equals("14"))
                i++;
            item.UpdateState(false, true);
        }
        if (i == _banzaiTiles.Count)
            BanzaiEnd();
    }

    private void HandleMaxBanzaiTiles(Point coord, Team team)
    {
        if (team == Team.None)
            return;
        var items = _room.GetGameMap().GetCoordinatedItems(coord);
        foreach (var item in _banzaiTiles.Values.ToList())
        {
            if (item == null)
                continue;
            if (item.GetBaseItem().InteractionType != InteractionType.Banzaifloor)
                continue;
            if (item.GetX != coord.X || item.GetY != coord.Y)
                continue;
            SetMaxForTile(item, team);
            _room.GetGameManager().AddPointToTeam(team, 1);
            item.UpdateState(false, true);
        }
    }

    private static void SetMaxForTile(Item item, Team team)
    {
        if (item.Value < 3)
        {
            item.Value = 3;
            item.Team = team;
        }
        var newColor = item.Value + Convert.ToInt32(item.Team) * 3 - 1;
        item.ExtraData = newColor.ToString();
    }

    public void Dispose()
    {
        _banzaiTiles.Clear();
        _pucks.Clear();
        if (_floorMap != null)
            Array.Clear(_floorMap, 0, _floorMap.Length);
        if (_field != null)
            _field.Dispose();
        _room = null;
        _banzaiTiles = null;
        _pucks = null;
        _floorMap = null;
        _field = null;
    }
}
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Rooms.Games.Teams;
using Plus.HabboHotel.Rooms.PathFinding;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Rooms.Games.Football;

public class Soccer
{
    private ConcurrentDictionary<int, Item> _balls;
    private Item[] _gates;
    private Room _room;

    public Soccer(Room room)
    {
        _room = room;
        _gates = new Item[4];
        _balls = new ConcurrentDictionary<int, Item>();
        GameIsStarted = false;
    }

    public bool GameIsStarted { get; private set; }

    public void StopGame(bool triggeredByUser = false)
    {
        GameIsStarted = false;
        if (!triggeredByUser)
            _room.GetWired().TriggerEvent(WiredBoxType.TriggerGameEnds, null);
    }

    public void StartGame()
    {
        GameIsStarted = true;
    }

    public void AddBall(Item item)
    {
        _balls.TryAdd(item.Id, item);
    }

    public void RemoveBall(int itemId)
    {
        _balls.TryRemove(itemId, out var item);
    }

    public void OnUserWalk(RoomUser user)
    {
        if (user == null)
            return;
        foreach (var item in _balls.Values.ToList())
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
                     VerifyBall(user, item.Coordinate.X, item.Coordinate.Y)) //VERYFIC BALL CHECAR SI ESTA EN DIRECCION ASIA LA PELOTA
            {
                newX = differenceX * -1;
                newY = differenceY * -1;
                newX = newX + item.GetX;
                newY = newY + item.GetY;
            }
            if (item.GetRoom().GetGameMap().ValidTile(newX, newY)) MoveBall(item, newX, newY, user);
        }
    }

    private bool VerifyBall(RoomUser user, int actualx, int actualy) => Rotation.Calculate(user.X, user.Y, actualx, actualy) == user.RotBody;

    public void RegisterGate(Item item)
    {
        if (_gates[0] == null)
        {
            item.Team = Team.Blue;
            _gates[0] = item;
        }
        else if (_gates[1] == null)
        {
            item.Team = Team.Red;
            _gates[1] = item;
        }
        else if (_gates[2] == null)
        {
            item.Team = Team.Green;
            _gates[2] = item;
        }
        else if (_gates[3] == null)
        {
            item.Team = Team.Yellow;
            _gates[3] = item;
        }
    }

    public void UnRegisterGate(Item item)
    {
        switch (item.Team)
        {
            case Team.Blue:
                {
                    _gates[0] = null;
                    break;
                }
            case Team.Red:
                {
                    _gates[1] = null;
                    break;
                }
            case Team.Green:
                {
                    _gates[2] = null;
                    break;
                }
            case Team.Yellow:
                {
                    _gates[3] = null;
                    break;
                }
        }
    }

    public void OnGateRemove(Item item)
    {
        switch (item.GetBaseItem().InteractionType)
        {
            case InteractionType.FootballGoalRed:
            case InteractionType.Footballcounterred:
                {
                    _room.GetGameManager().RemoveFurnitureFromTeam(item, Team.Red);
                    break;
                }
            case InteractionType.FootballGoalGreen:
            case InteractionType.Footballcountergreen:
                {
                    _room.GetGameManager().RemoveFurnitureFromTeam(item, Team.Green);
                    break;
                }
            case InteractionType.FootballGoalBlue:
            case InteractionType.Footballcounterblue:
                {
                    _room.GetGameManager().RemoveFurnitureFromTeam(item, Team.Blue);
                    break;
                }
            case InteractionType.FootballGoalYellow:
            case InteractionType.Footballcounteryellow:
                {
                    _room.GetGameManager().RemoveFurnitureFromTeam(item, Team.Yellow);
                    break;
                }
        }
    }

    public void MoveBall(Item item, int newX, int newY, RoomUser user)
    {
        if (item == null || user == null)
            return;
        if (!_room.GetGameMap().ItemCanBePlaced(newX, newY))
            return;
        var oldRoomCoord = item.Coordinate;
        if (oldRoomCoord.X == newX && oldRoomCoord.Y == newY)
            return;
        double newZ = _room.GetGameMap().Model.SqFloorHeight[newX, newY];
        _room.SendPacket(new SlideObjectBundleComposer(item.Coordinate.X, item.Coordinate.Y, item.GetZ, newX, newY, newZ, item.Id, item.Id, item.Id));
        item.ExtraData = "11";
        item.UpdateNeeded = true;
        _room.GetRoomItemHandler().SetFloorItem(null, item, newX, newY, item.Rotation, false, false, false);
        _room.OnUserShoot(user, item);
    }

    public void Dispose()
    {
        Array.Clear(_gates, 0, _gates.Length);
        _gates = null;
        _room = null;
        _balls.Clear();
        _balls = null;
    }
}
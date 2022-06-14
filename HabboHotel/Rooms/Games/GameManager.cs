using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms.Games.Teams;
using System.Collections.Concurrent;
using System.Drawing;

namespace Plus.HabboHotel.Rooms.Games;

public class GameManager
{
    private ConcurrentDictionary<int, Item> _blueTeamItems;
    private ConcurrentDictionary<int, Item> _greenTeamItems;
    private ConcurrentDictionary<int, Item> _redTeamItems;
    private Room _room;
    private ConcurrentDictionary<int, Item> _yellowTeamItems;

    public GameManager(Room room)
    {
        _room = room;
        Points = new int[5];
        _redTeamItems = new ConcurrentDictionary<int, Item>();
        _blueTeamItems = new ConcurrentDictionary<int, Item>();
        _greenTeamItems = new ConcurrentDictionary<int, Item>();
        _yellowTeamItems = new ConcurrentDictionary<int, Item>();
    }

    public int[] Points { get; set; }

    public Team GetWinningTeam()
    {
        var winning = 1;
        var highestScore = 0;
        for (var i = 1; i < 5; i++)
        {
            if (Points[i] > highestScore)
            {
                highestScore = Points[i];
                winning = i;
            }
        }
        return (Team)winning;
    }

    public void AddPointToTeam(Team team, int points)
    {
        var newPoints = Points[Convert.ToInt32(team)] += points;
        if (newPoints < 0)
            newPoints = 0;
        Points[Convert.ToInt32(team)] = newPoints;
        foreach (var item in GetFurniItems(team).Values.ToList())
        {
            if (!IsFootballGoal(item.GetBaseItem().InteractionType))
            {
                item.ExtraData = Points[Convert.ToInt32(team)].ToString();
                item.UpdateState();
            }
        }
        foreach (var item in _room.GetRoomItemHandler().GetFloor.ToList())
        {
            if (team == Team.Blue && item.Data.InteractionType == InteractionType.Banzaiscoreblue)
            {
                item.ExtraData = Points[Convert.ToInt32(team)].ToString();
                item.UpdateState();
            }
            else if (team == Team.Red && item.Data.InteractionType == InteractionType.Banzaiscorered)
            {
                item.ExtraData = Points[Convert.ToInt32(team)].ToString();
                item.UpdateState();
            }
            else if (team == Team.Green && item.Data.InteractionType == InteractionType.Banzaiscoregreen)
            {
                item.ExtraData = Points[Convert.ToInt32(team)].ToString();
                item.UpdateState();
            }
            else if (team == Team.Yellow && item.Data.InteractionType == InteractionType.Banzaiscoreyellow)
            {
                item.ExtraData = Points[Convert.ToInt32(team)].ToString();
                item.UpdateState();
            }
        }
    }

    public void Reset()
    {
        AddPointToTeam(Team.Blue, GetScoreForTeam(Team.Blue) * -1);
        AddPointToTeam(Team.Green, GetScoreForTeam(Team.Green) * -1);
        AddPointToTeam(Team.Red, GetScoreForTeam(Team.Red) * -1);
        AddPointToTeam(Team.Yellow, GetScoreForTeam(Team.Yellow) * -1);
    }

    private int GetScoreForTeam(Team team) => Points[Convert.ToInt32(team)];

    private ConcurrentDictionary<int, Item> GetFurniItems(Team team)
    {
        switch (team)
        {
            default:
                return new ConcurrentDictionary<int, Item>();
            case Team.Blue:
                return _blueTeamItems;
            case Team.Green:
                return _greenTeamItems;
            case Team.Red:
                return _redTeamItems;
            case Team.Yellow:
                return _yellowTeamItems;
        }
    }

    private static bool IsFootballGoal(InteractionType type) => type == InteractionType.FootballGoalBlue || type == InteractionType.FootballGoalGreen || type == InteractionType.FootballGoalRed ||
                                                                type == InteractionType.FootballGoalYellow;

    public void AddFurnitureToTeam(Item item, Team team)
    {
        switch (team)
        {
            case Team.Blue:
                _blueTeamItems.TryAdd(item.Id, item);
                break;
            case Team.Green:
                _greenTeamItems.TryAdd(item.Id, item);
                break;
            case Team.Red:
                _redTeamItems.TryAdd(item.Id, item);
                break;
            case Team.Yellow:
                _yellowTeamItems.TryAdd(item.Id, item);
                break;
        }
    }

    public void RemoveFurnitureFromTeam(Item item, Team team)
    {
        switch (team)
        {
            case Team.Blue:
                _blueTeamItems.TryRemove(item.Id, out item);
                break;
            case Team.Green:
                _greenTeamItems.TryRemove(item.Id, out item);
                break;
            case Team.Red:
                _redTeamItems.TryRemove(item.Id, out item);
                break;
            case Team.Yellow:
                _yellowTeamItems.TryRemove(item.Id, out item);
                break;
        }
    }

    public void LockGates()
    {
        foreach (var item in _redTeamItems.Values.ToList()) LockGate(item);
        foreach (var item in _greenTeamItems.Values.ToList()) LockGate(item);
        foreach (var item in _blueTeamItems.Values.ToList()) LockGate(item);
        foreach (var item in _yellowTeamItems.Values.ToList()) LockGate(item);
    }

    public void UnlockGates()
    {
        foreach (var item in _redTeamItems.Values.ToList()) UnlockGate(item);
        foreach (var item in _greenTeamItems.Values.ToList()) UnlockGate(item);
        foreach (var item in _blueTeamItems.Values.ToList()) UnlockGate(item);
        foreach (var item in _yellowTeamItems.Values.ToList()) UnlockGate(item);
    }

    private void LockGate(Item item)
    {
        var type = item.GetBaseItem().InteractionType;
        if (type == InteractionType.FreezeBlueGate || type == InteractionType.FreezeGreenGate ||
            type == InteractionType.FreezeRedGate || type == InteractionType.FreezeYellowGate
            || type == InteractionType.Banzaigateblue || type == InteractionType.Banzaigatered ||
            type == InteractionType.Banzaigategreen || type == InteractionType.Banzaigateyellow)
        {
            foreach (var user in _room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY))) user.SqState = 0;
            _room.GetGameMap().GameMap[item.GetX, item.GetY] = 0;
        }
    }

    private void UnlockGate(Item item)
    {
        var type = item.GetBaseItem().InteractionType;
        if (type == InteractionType.FreezeBlueGate || type == InteractionType.FreezeGreenGate ||
            type == InteractionType.FreezeRedGate || type == InteractionType.FreezeYellowGate
            || type == InteractionType.Banzaigateblue || type == InteractionType.Banzaigatered ||
            type == InteractionType.Banzaigategreen || type == InteractionType.Banzaigateyellow)
        {
            foreach (var user in _room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY))) user.SqState = 1;
            _room.GetGameMap().GameMap[item.GetX, item.GetY] = 1;
        }
    }

    public void StopGame()
    {
        _room.LastTimerReset = DateTime.Now;
    }

    public void Dispose()
    {
        Array.Clear(Points, 0, Points.Length);
        _redTeamItems.Clear();
        _blueTeamItems.Clear();
        _greenTeamItems.Clear();
        _yellowTeamItems.Clear();
        Points = null;
        _redTeamItems = null;
        _blueTeamItems = null;
        _greenTeamItems = null;
        _yellowTeamItems = null;
        _room = null;
    }
}
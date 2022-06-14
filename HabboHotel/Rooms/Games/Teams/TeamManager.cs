using Plus.HabboHotel.Items;
using System.Drawing;

namespace Plus.HabboHotel.Rooms.Games.Teams;

public class TeamManager
{
    public List<RoomUser> BlueTeam;
    public string Game;
    public List<RoomUser> GreenTeam;
    public List<RoomUser> RedTeam;
    public List<RoomUser> YellowTeam;

    public static TeamManager CreateTeam(string game)
    {
        var t = new TeamManager();
        t.Game = game;
        t.BlueTeam = new List<RoomUser>();
        t.RedTeam = new List<RoomUser>();
        t.GreenTeam = new List<RoomUser>();
        t.YellowTeam = new List<RoomUser>();
        return t;
    }

    public bool CanEnterOnTeam(Team t)
    {
        if (t.Equals(Team.Blue))
            return BlueTeam.Count < 5;
        if (t.Equals(Team.Red))
            return RedTeam.Count < 5;
        if (t.Equals(Team.Yellow))
            return YellowTeam.Count < 5;
        if (t.Equals(Team.Green))
            return GreenTeam.Count < 5;
        return false;
    }

    public void AddUser(RoomUser user)
    {
        if (user.Team.Equals(Team.Blue) && !BlueTeam.Contains(user))
            BlueTeam.Add(user);
        else if (user.Team.Equals(Team.Red) && !RedTeam.Contains(user))
            RedTeam.Add(user);
        else if (user.Team.Equals(Team.Yellow) && !YellowTeam.Contains(user))
            YellowTeam.Add(user);
        else if (user.Team.Equals(Team.Green) && !GreenTeam.Contains(user))
            GreenTeam.Add(user);
        switch (Game.ToLower())
        {
            case "banzai":
                {
                    var room = user.GetClient().GetHabbo().CurrentRoom;
                    if (room == null)
                        return;
                    foreach (var item in room.GetRoomItemHandler().GetFloor.ToList())
                    {
                        if (item == null)
                            continue;
                        if (item.GetBaseItem().InteractionType.Equals(InteractionType.Banzaigateblue))
                        {
                            item.ExtraData = BlueTeam.Count.ToString();
                            item.UpdateState();
                            if (BlueTeam.Count == 5)
                            {
                                foreach (var sser in room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY))) sser.SqState = 0;
                                room.GetGameMap().GameMap[item.GetX, item.GetY] = 0;
                            }
                        }
                        else if (item.GetBaseItem().InteractionType.Equals(InteractionType.Banzaigatered))
                        {
                            item.ExtraData = RedTeam.Count.ToString();
                            item.UpdateState();
                            if (RedTeam.Count == 5)
                            {
                                foreach (var sser in room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY))) sser.SqState = 0;
                                room.GetGameMap().GameMap[item.GetX, item.GetY] = 0;
                            }
                        }
                        else if (item.GetBaseItem().InteractionType.Equals(InteractionType.Banzaigategreen))
                        {
                            item.ExtraData = GreenTeam.Count.ToString();
                            item.UpdateState();
                            if (GreenTeam.Count == 5)
                            {
                                foreach (var sser in room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                    sser.SqState = 0;
                                room.GetGameMap().GameMap[item.GetX, item.GetY] = 0;
                            }
                        }
                        else if (item.GetBaseItem().InteractionType.Equals(InteractionType.Banzaigateyellow))
                        {
                            item.ExtraData = YellowTeam.Count.ToString();
                            item.UpdateState();
                            if (YellowTeam.Count == 5)
                            {
                                foreach (var sser in room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                    sser.SqState = 0;
                                room.GetGameMap().GameMap[item.GetX, item.GetY] = 0;
                            }
                        }
                    }
                    break;
                }
            case "freeze":
                {
                    var room = user.GetClient().GetHabbo().CurrentRoom;
                    if (room == null)
                        return;
                    foreach (var item in room.GetRoomItemHandler().GetFloor.ToList())
                    {
                        if (item == null)
                            continue;
                        if (item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeBlueGate))
                        {
                            item.ExtraData = BlueTeam.Count.ToString();
                            item.UpdateState();
                        }
                        else if (item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeRedGate))
                        {
                            item.ExtraData = RedTeam.Count.ToString();
                            item.UpdateState();
                        }
                        else if (item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeGreenGate))
                        {
                            item.ExtraData = GreenTeam.Count.ToString();
                            item.UpdateState();
                        }
                        else if (item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeYellowGate))
                        {
                            item.ExtraData = YellowTeam.Count.ToString();
                            item.UpdateState();
                        }
                    }
                    break;
                }
        }
    }

    public void OnUserLeave(RoomUser user)
    {
        //Console.WriteLine("remove user from team! (" + Game + ")");
        if (user.Team.Equals(Team.Blue) && BlueTeam.Contains(user))
            BlueTeam.Remove(user);
        else if (user.Team.Equals(Team.Red) && RedTeam.Contains(user))
            RedTeam.Remove(user);
        else if (user.Team.Equals(Team.Yellow) && YellowTeam.Contains(user))
            YellowTeam.Remove(user);
        else if (user.Team.Equals(Team.Green) && GreenTeam.Contains(user))
            GreenTeam.Remove(user);
        switch (Game.ToLower())
        {
            case "banzai":
                {
                    var room = user.GetClient().GetHabbo().CurrentRoom;
                    if (room == null)
                        return;
                    foreach (var item in room.GetRoomItemHandler().GetFloor.ToList())
                    {
                        if (item == null)
                            continue;
                        if (item.GetBaseItem().InteractionType.Equals(InteractionType.Banzaigateblue))
                        {
                            item.ExtraData = BlueTeam.Count.ToString();
                            item.UpdateState();
                            if (room.GetGameMap().GameMap[item.GetX, item.GetY] == 0)
                            {
                                foreach (var sser in room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                    sser.SqState = 1;
                                room.GetGameMap().GameMap[item.GetX, item.GetY] = 1;
                            }
                        }
                        else if (item.GetBaseItem().InteractionType.Equals(InteractionType.Banzaigatered))
                        {
                            item.ExtraData = RedTeam.Count.ToString();
                            item.UpdateState();
                            if (room.GetGameMap().GameMap[item.GetX, item.GetY] == 0)
                            {
                                foreach (var sser in room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                    sser.SqState = 1;
                                room.GetGameMap().GameMap[item.GetX, item.GetY] = 1;
                            }
                        }
                        else if (item.GetBaseItem().InteractionType.Equals(InteractionType.Banzaigategreen))
                        {
                            item.ExtraData = GreenTeam.Count.ToString();
                            item.UpdateState();
                            if (room.GetGameMap().GameMap[item.GetX, item.GetY] == 0)
                            {
                                foreach (var sser in room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                    sser.SqState = 1;
                                room.GetGameMap().GameMap[item.GetX, item.GetY] = 1;
                            }
                        }
                        else if (item.GetBaseItem().InteractionType.Equals(InteractionType.Banzaigateyellow))
                        {
                            item.ExtraData = YellowTeam.Count.ToString();
                            item.UpdateState();
                            if (room.GetGameMap().GameMap[item.GetX, item.GetY] == 0)
                            {
                                foreach (var sser in room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                    sser.SqState = 1;
                                room.GetGameMap().GameMap[item.GetX, item.GetY] = 1;
                            }
                        }
                    }
                    break;
                }
            case "freeze":
                {
                    var room = user.GetClient().GetHabbo().CurrentRoom;
                    if (room == null)
                        return;
                    foreach (var item in room.GetRoomItemHandler().GetFloor.ToList())
                    {
                        if (item == null)
                            continue;
                        if (item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeBlueGate))
                        {
                            item.ExtraData = BlueTeam.Count.ToString();
                            item.UpdateState();
                        }
                        else if (item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeRedGate))
                        {
                            item.ExtraData = RedTeam.Count.ToString();
                            item.UpdateState();
                        }
                        else if (item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeGreenGate))
                        {
                            item.ExtraData = GreenTeam.Count.ToString();
                            item.UpdateState();
                        }
                        else if (item.GetBaseItem().InteractionType.Equals(InteractionType.FreezeYellowGate))
                        {
                            item.ExtraData = YellowTeam.Count.ToString();
                            item.UpdateState();
                        }
                    }
                    break;
                }
        }
    }

    public void Dispose()
    {
        BlueTeam.Clear();
        GreenTeam.Clear();
        RedTeam.Clear();
        YellowTeam.Clear();
    }
}
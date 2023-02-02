using System.Collections.Concurrent;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Core;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.Games.Teams;
using Plus.HabboHotel.Rooms.PathFinding;
using Plus.HabboHotel.Rooms.Trading;
using Plus.Utilities;

using Dapper;

namespace Plus.HabboHotel.Rooms;

public class RoomUserManager
{
    private ConcurrentDictionary<int, RoomUser> _bots;
    private ConcurrentDictionary<int, RoomUser> _pets;

    private int _primaryPrivateUserId;
    private Room _room;
    private int _secondaryPrivateUserId;
    private ConcurrentDictionary<int, RoomUser> _users;

    public int UserCount;


    public RoomUserManager(Room room)
    {
        _room = room;
        _users = new();
        _pets = new();
        _bots = new();
        _primaryPrivateUserId = 0;
        _secondaryPrivateUserId = 0;
        PetCount = 0;
        UserCount = 0;
    }

    public int PetCount { get; private set; }

    public RoomUser DeployBot(RoomBot bot, Pet pet)
    {
        var user = new RoomUser(0, _room.RoomId, _primaryPrivateUserId++, _room);
        bot.VirtualId = _primaryPrivateUserId;
        var personalId = _secondaryPrivateUserId++;
        user.InternalRoomId = personalId;
        _users.TryAdd(personalId, user);
        var model = _room.GetGameMap().Model;
        if (bot.X > 0 && bot.Y > 0 && bot.X < model.MapSizeX && bot.Y < model.MapSizeY)
        {
            user.SetPos(bot.X, bot.Y, bot.Z);
            user.SetRot(bot.Rot, false);
        }
        else
        {
            bot.X = model.DoorX;
            bot.Y = model.DoorY;
            user.SetPos(model.DoorX, model.DoorY, model.DoorZ);
            user.SetRot(model.DoorOrientation, false);
        }
        user.BotData = bot;
        user.BotAi = bot.GenerateBotAi(user.VirtualId);
        if (user.IsPet)
        {
            user.BotAi.Init(bot.BotId, user.VirtualId, _room.RoomId, user, _room);
            user.PetData = pet;
            user.PetData.VirtualId = user.VirtualId;
        }
        else
            user.BotAi.Init(bot.BotId, user.VirtualId, _room.RoomId, user, _room);
        user.UpdateNeeded = true;
        _room.SendPacket(new UsersComposer(user));
        if (user.IsPet)
        {
            if (_pets.ContainsKey(user.PetData.PetId))
                _pets[user.PetData.PetId] = user;
            else
                _pets.TryAdd(user.PetData.PetId, user);
            PetCount++;
        }
        else if (user.IsBot)
        {
            if (_bots.ContainsKey(user.BotData.BotId))
                _bots[user.BotData.BotId] = user;
            else
                _bots.TryAdd(user.BotData.Id, user);
            _room.SendPacket(new DanceComposer(user, user.BotData.DanceId));
        }
        return user;
    }

    public void RemoveBot(int virtualId, bool kicked)
    {
        var user = GetRoomUserByVirtualId(virtualId);
        if (user == null || !user.IsBot)
            return;
        if (user.IsPet)
        {
            _pets.TryRemove(user.PetData.PetId, out var pet);
            PetCount--;
        }
        else
            _bots.TryRemove(user.BotData.Id, out var bot);
        user.BotAi.OnSelfLeaveRoom(kicked);
        _room.SendPacket(new UserRemoveComposer(user.VirtualId));
        if (_users != null)
            _users.TryRemove(user.InternalRoomId, out var toRemove);
        OnRemove(user);
    }

    public RoomUser GetUserForSquare(int x, int y) => _room.GetGameMap().GetRoomUsers(new(x, y)).FirstOrDefault();

    public bool AddAvatarToRoom(GameClient session)
    {
        if (_room == null)
            return false;
        if (session == null)
            return false;
        if (session.GetHabbo().CurrentRoom == null)
            return false;
        if (_users.Any(u => u.Value.UserId == session.GetHabbo().Id))
            return false;
        var user = new RoomUser(session.GetHabbo().Id, _room.RoomId, _primaryPrivateUserId++, _room);
        if (user == null || user.GetClient() == null)
            return false;
        user.UserId = session.GetHabbo().Id;
        session.GetHabbo().TentId = 0;
        var personalId = _secondaryPrivateUserId++;
        user.InternalRoomId = personalId;
        session.GetHabbo().CurrentRoom = _room;
        if (!_users.TryAdd(personalId, user))
            return false;
        var model = _room.GetGameMap().Model;
        if (model == null)
            return false;
        if (!_room.PetMorphsAllowed && session.GetHabbo().PetId != 0)
            session.GetHabbo().PetId = 0;
        if (!session.GetHabbo().IsTeleporting && !session.GetHabbo().IsHopping)
        {
            if (!model.DoorIsValid())
            {
                var square = _room.GetGameMap().GetRandomWalkableSquare();
                model.DoorX = square.X;
                model.DoorY = square.Y;
                model.DoorZ = (int)_room.GetGameMap().GetHeightForSquareFromData(square);
            }
            user.SetPos(model.DoorX, model.DoorY, model.DoorZ);
            user.SetRot(model.DoorOrientation, false);
        }
        else if (!user.IsBot && (user.GetClient().GetHabbo().IsTeleporting || user.GetClient().GetHabbo().IsHopping))
        {
            Item item = null;
            if (session.GetHabbo().IsTeleporting)
                item = _room.GetRoomItemHandler().GetItem(session.GetHabbo().TeleporterId);
            else if (session.GetHabbo().IsHopping)
                item = _room.GetRoomItemHandler().GetItem(session.GetHabbo().HopperId);
            if (item != null)
            {
                if (session.GetHabbo().IsTeleporting)
                {
                    item.LegacyDataString = "2";
                    item.UpdateState(false, true);
                    user.SetPos(item.GetX, item.GetY, item.GetZ);
                    user.SetRot(item.Rotation, false);
                    item.InteractingUser2 = session.GetHabbo().Id;
                    item.LegacyDataString = "0";
                    item.UpdateState(false, true);
                }
                else if (session.GetHabbo().IsHopping)
                {
                    item.LegacyDataString = "1";
                    item.UpdateState(false, true);
                    user.SetPos(item.GetX, item.GetY, item.GetZ);
                    user.SetRot(item.Rotation, false);
                    user.AllowOverride = false;
                    item.InteractingUser2 = session.GetHabbo().Id;
                    item.LegacyDataString = "2";
                    item.UpdateState(false, true);
                }
            }
            else
            {
                user.SetPos(model.DoorX, model.DoorY, model.DoorZ - 1);
                user.SetRot(model.DoorOrientation, false);
            }
        }
        _room.SendPacket(new UsersComposer(user));
        if (_room.CheckRights(session, true))
        {
            user.SetStatus("flatctrl", "useradmin");
            session.Send(new YouAreOwnerComposer());
            session.Send(new YouAreControllerComposer(4));
        }
        else if (_room.CheckRights(session, false) && _room.Group == null)
        {
            user.SetStatus("flatctrl", "1");
            session.Send(new YouAreControllerComposer(1));
        }
        else if (_room.Group != null && _room.CheckRights(session, false, true))
        {
            user.SetStatus("flatctrl", "3");
            session.Send(new YouAreControllerComposer(3));
        }
        else
            session.Send(new YouAreNotControllerComposer());
        user.UpdateNeeded = true;
        if (session.GetHabbo().Permissions.HasRight("mod_tool") && !session.GetHabbo().DisableForcedEffects)
            session.GetHabbo().Effects.ApplyEffect(102);
        if (session.GetHabbo().IsAmbassador && !session.GetHabbo().DisableForcedEffects && !session.GetHabbo().Permissions.HasRight("mod_tool"))
            session.GetHabbo().Effects.ApplyEffect(178);
        foreach (var bot in _bots.Values.ToList())
        {
            if (bot == null || bot.BotAi == null)
                continue;
            bot.BotAi.OnUserEnterRoom(user);
        }
        return true;
    }

    public void RemoveUserFromRoom(GameClient session, bool nofityUser, bool notifyKick = false)
    {
        try
        {
            if (_room == null)
                return;
            if (session == null || session.GetHabbo() == null)
                return;
            if (notifyKick)
                session.Send(new GenericErrorComposer(4008));
            if (nofityUser)
                session.Send(new CloseConnectionComposer());
            if (session.GetHabbo().TentId > 0)
                session.GetHabbo().TentId = 0;
            session.GetHabbo().CurrentRoom = null;
            var user = GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user != null)
            {
                if (user.RidingHorse)
                {
                    user.RidingHorse = false;
                    var userRiding = GetRoomUserByVirtualId(user.HorseId);
                    if (userRiding != null)
                    {
                        userRiding.RidingHorse = false;
                        userRiding.HorseId = 0;
                    }
                }
                if (user.Team != Team.None)
                {
                    var team = _room.GetTeamManagerForFreeze();
                    if (team != null)
                    {
                        team.OnUserLeave(user);
                        user.Team = Team.None;
                        if (user.GetClient().GetHabbo().Effects.CurrentEffect != 0)
                            user.GetClient().GetHabbo().Effects.ApplyEffect(0);
                    }
                }
                RemoveRoomUser(user);
                if (user.CurrentItemEffect != ItemEffectType.None)
                {
                    if (session.GetHabbo().Effects != null)
                        session.GetHabbo().Effects.CurrentEffect = -1;
                }
                if (user.IsTrading)
                {
                    Trade trade = null;
                    if (_room.GetTrading().TryGetTrade(user.TradeId, out trade))
                        trade.EndTrade(user.TradeId);
                }

                //Session.GetHabbo().CurrentRoomId = 0;
                    session.GetHabbo().Messenger?.NotifyChangesToFriends();
                using (var dbClient = PlusEnvironment.DatabaseManager.Connection())
                {
                    dbClient.Execute("UPDATE user_roomvisits SET exit_timestamp = @exitTimestamp WHERE room_id = @roomId AND user_id = @userId ORDER BY exit_timestamp DESC LIMIT 1",
                        new
                        {
                            userId = session.GetHabbo().Id,
                            roomId = _room.RoomId,
                            exitTimestamp = UnixTimestamp.GetNow(),
                        });

                    dbClient.Execute("UPDATE `rooms` SET `users_now` = @usersNow WHERE `id` = @roomId LIMIT 1",
                        new
                        {
                            usersNow = _room.UsersNow,
                            roomId = _room.RoomId
                        });
                }
                if (user != null)
                    user.Dispose();
            }
        }
        catch (Exception e)
        {
            ExceptionLogger.LogException(e);
        }
    }

    private void OnRemove(RoomUser user)
    {
        try
        {
            var session = user.GetClient();
            if (session == null)
                return;
            var bots = new List<RoomUser>();
            try
            {
                foreach (var roomUser in GetUserList().ToList())
                {
                    if (roomUser == null)
                        continue;
                    if (roomUser.IsBot && !roomUser.IsPet)
                    {
                        if (!bots.Contains(roomUser))
                            bots.Add(roomUser);
                    }
                }
            }
            catch { }
            var petsToRemove = new List<RoomUser>();
            foreach (var bot in bots.ToList())
            {
                if (bot == null || bot.BotAi == null)
                    continue;
                bot.BotAi.OnUserLeaveRoom(session);
                if (bot.IsPet && bot.PetData.OwnerId == user.UserId && !_room.CheckRights(session, true))
                {
                    if (!petsToRemove.Contains(bot))
                        petsToRemove.Add(bot);
                }
            }
            foreach (var toRemove in petsToRemove.ToList())
            {
                if (toRemove == null)
                    continue;
                if (user.GetClient() == null || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().Inventory == null)
                    continue;
                if (user.GetClient().GetHabbo().Inventory.Pets.AddPet(toRemove.PetData))
                {
                    toRemove.PetData.RoomId = 0;
                    toRemove.PetData.PlacedInRoom = false;
                    RemoveBot(toRemove.VirtualId, false);
                }
            }
            _room.GetGameMap().RemoveUserFromMap(user, new(user.X, user.Y));
        }
        catch (Exception e)
        {
            ExceptionLogger.LogCriticalException(e);
        }
    }

    private void RemoveRoomUser(RoomUser user)
    {
        if (user.SetStep)
            _room.GetGameMap().GameMap[user.SetX, user.SetY] = user.SqState;
        else
            _room.GetGameMap().GameMap[user.X, user.Y] = user.SqState;
        _room.GetGameMap().RemoveUserFromMap(user, new(user.X, user.Y));
        _room.SendPacket(new UserRemoveComposer(user.VirtualId));
        RoomUser toRemove = null;
        if (_users.TryRemove(user.InternalRoomId, out toRemove))
        {
            //uhmm, could put the below stuff in but idk.
        }
        user.InternalRoomId = -1;
        OnRemove(user);
    }

    public bool TryGetPet(int petId, out RoomUser pet) => _pets.TryGetValue(petId, out pet);

    public bool TryGetBot(int botId, out RoomUser bot) => _bots.TryGetValue(botId, out bot);

    public RoomUser GetBotByName(string name)
    {
        var foundBot = _bots.Count(x => x.Value.BotData != null && x.Value.BotData.Name.ToLower() == name.ToLower()) > 0;
        if (foundBot)
        {
            var id = _bots.FirstOrDefault(x => x.Value.BotData != null && x.Value.BotData.Name.ToLower() == name.ToLower()).Value.BotData.Id;
            return _bots[id];
        }
        return null;
    }

    public void UpdateUserCount(int count)
    {
        UserCount = count;
        _room.UsersNow = count;
        using var dbClient = PlusEnvironment.DatabaseManager.GetQueryReactor();
        dbClient.RunQuery($"UPDATE `rooms` SET `users_now` = '{count}' WHERE `id` = '{_room.RoomId}' LIMIT 1");
    }

    public RoomUser GetRoomUserByVirtualId(int virtualId)
    {
        RoomUser user = null;
        if (!_users.TryGetValue(virtualId, out user))
            return null;
        return user;
    }

    public RoomUser GetRoomUserByHabbo(int id)
    {
        var user = GetUserList().Where(x => x != null && x.GetClient() != null && x.GetClient().GetHabbo() != null && x.GetClient().GetHabbo().Id == id).FirstOrDefault();
        if (user != null)
            return user;
        return null;
    }

    public List<RoomUser> GetRoomUsers()
    {
        var list = new List<RoomUser>();
        list = GetUserList().Where(x => !x.IsBot).ToList();
        return list;
    }

    public List<RoomUser> GetRoomUserByRank(int minRank)
    {
        var returnList = new List<RoomUser>();
        foreach (var user in GetUserList().ToList())
        {
            if (user == null)
                continue;
            if (!user.IsBot && user.GetClient() != null && user.GetClient().GetHabbo() != null && user.GetClient().GetHabbo().Rank >= minRank)
                returnList.Add(user);
        }
        return returnList;
    }

    public RoomUser GetRoomUserByHabbo(string pName)
    {
        var user = GetUserList().FirstOrDefault(x =>
            x != null && x.GetClient() != null && x.GetClient().GetHabbo() != null && x.GetClient().GetHabbo().Username.Equals(pName, StringComparison.OrdinalIgnoreCase));
        if (user != null)
            return user;
        return null;
    }

    public void UpdatePets()
    {
        using var dbClient = PlusEnvironment.DatabaseManager.GetQueryReactor();
        foreach (var pet in GetPets().ToList())
        {
            if (pet == null)
                continue;
            if (pet.DbState == PetDatabaseUpdateState.NeedsInsert)
            {
                dbClient.SetQuery($"INSERT INTO `bots` (`id`,`user_id`,`room_id`,`name`,`x`,`y`,`z`) VALUES ('{pet.PetId}','{pet.OwnerId}','{pet.RoomId}',@name,'0','0','0')");
                dbClient.AddParameter("name", pet.Name);
                dbClient.RunQuery();
                dbClient.SetQuery(
                    $"INSERT INTO `bots_petdata` (`type`,`race`,`color`,`experience`,`energy`,`createstamp`,`nutrition`,`respect`) VALUES ('{pet.Type}',@race,@color,'0','100','{pet.CreationStamp}','0','0')");
                dbClient.AddParameter($"{pet.PetId}race", pet.Race);
                dbClient.AddParameter($"{pet.PetId}color", pet.Color);
                dbClient.RunQuery();
            }
            else if (pet.DbState == PetDatabaseUpdateState.NeedsUpdate)
            {
                //Surely this can be *99 better? // TODO
                var user = GetRoomUserByVirtualId(pet.VirtualId);
                dbClient.RunQuery($"UPDATE `bots` SET room_id = {pet.RoomId}, x = {(user?.X ?? 0)}, Y = {(user?.Y ?? 0)}, Z = {(user?.Z ?? 0)} WHERE `id` = '{pet.PetId}' LIMIT 1");
                dbClient.RunQuery(
                    $"UPDATE `bots_petdata` SET `experience` = '{pet.Experience}', `energy` = '{pet.Energy}', `nutrition` = '{pet.Nutrition}', `respect` = '{pet.Respect}' WHERE `id` = '{pet.PetId}' LIMIT 1");
            }
            pet.DbState = PetDatabaseUpdateState.Updated;
        }
    }

    private void UpdateBots()
    {
        using var dbClient = PlusEnvironment.DatabaseManager.GetQueryReactor();
        foreach (var user in GetRoomUsers().ToList())
        {
            if (user == null || !user.IsBot)
                continue;
            if (user.IsBot)
            {
                dbClient.SetQuery("UPDATE bots SET x=@x, y=@y, z=@z, name=@name, look=@look, rotation=@rotation WHERE id=@id LIMIT 1;");
                dbClient.AddParameter("name", user.BotData.Name);
                dbClient.AddParameter("look", user.BotData.Look);
                dbClient.AddParameter("rotation", user.BotData.Rot);
                dbClient.AddParameter("x", user.X);
                dbClient.AddParameter("y", user.Y);
                dbClient.AddParameter("z", user.Z);
                dbClient.AddParameter("id", user.BotData.BotId);
                dbClient.RunQuery();
            }
        }
    }


    public List<Pet> GetPets()
    {
        var pets = new List<Pet>();
        foreach (var user in _pets.Values.ToList())
        {
            if (user == null || !user.IsPet)
                continue;
            pets.Add(user.PetData);
        }
        return pets;
    }

    public void SerializeStatusUpdates()
    {
        var users = new List<RoomUser>();
        var roomUsers = GetUserList();
        if (roomUsers == null)
            return;
        foreach (var user in roomUsers.ToList())
        {
            if (user == null || !user.UpdateNeeded || users.Contains(user))
                continue;
            user.UpdateNeeded = false;
            users.Add(user);
        }
        if (users.Count > 0)
            _room.SendPacket(new UserUpdateComposer(users));
    }

    public void UpdateUserStatusses()
    {
        foreach (var user in GetUserList().ToList())
        {
            if (user == null)
                continue;
            UpdateUserStatus(user, false);
        }
    }

    private bool IsValid(RoomUser user)
    {
        if (user == null)
            return false;
        if (user.IsBot)
            return true;
        if (user.GetClient() == null)
            return false;
        if (user.GetClient().GetHabbo() == null)
            return false;
        if (user.GetClient().GetHabbo().CurrentRoom != _room)
            return false;
        return true;
    }

    public void OnCycle()
    {
        var userCounter = 0;
        try
        {
            var toRemove = new List<RoomUser>();
            foreach (var user in GetUserList().ToList())
            {
                if (user == null)
                    continue;
                if (!IsValid(user))
                {
                    if (user.GetClient() != null)
                        RemoveUserFromRoom(user.GetClient(), false);
                    else
                        RemoveRoomUser(user);
                }
                if (user.NeedsAutokick && !toRemove.Contains(user))
                {
                    toRemove.Add(user);
                    continue;
                }
                var updated = false;
                user.IdleTime++;
                user.HandleSpamTicks();
                if (!user.IsBot && !user.IsAsleep && user.IdleTime >= 600)
                {
                    user.IsAsleep = true;
                    _room.SendPacket(new SleepComposer(user, true));
                }
                if (user.CarryItemId > 0)
                {
                    user.CarryTimer--;
                    if (user.CarryTimer <= 0)
                        user.CarryItem(0);
                }
                if (_room.GotFreeze())
                    _room.GetFreeze().CycleUser(user);
                var invalidStep = false;
                if (user.IsRolling)
                {
                    if (user.RollerDelay <= 0)
                    {
                        UpdateUserStatus(user, false);
                        user.IsRolling = false;
                    }
                    else
                        user.RollerDelay--;
                }
                if (user.SetStep)
                {
                    if (_room.GetGameMap().IsValidStep2(user, new(user.X, user.Y), new(user.SetX, user.SetY), user.GoalX == user.SetX && user.GoalY == user.SetY, user.AllowOverride))
                    {
                        if (!user.RidingHorse)
                            _room.GetGameMap().UpdateUserMovement(new(user.Coordinate.X, user.Coordinate.Y), new(user.SetX, user.SetY), user);
                        var coordinatedItems = _room.GetGameMap().GetCoordinatedItems(new(user.X, user.Y));
                        foreach (var item in coordinatedItems.ToList()) item.UserWalksOffFurni(user);
                        if (!user.IsBot)
                        {
                            user.X = user.SetX;
                            user.Y = user.SetY;
                            user.Z = user.SetZ;
                        }
                        else if (user.IsBot && !user.RidingHorse)
                        {
                            user.X = user.SetX;
                            user.Y = user.SetY;
                            user.Z = user.SetZ;
                        }
                        if (!user.IsBot && user.RidingHorse)
                        {
                            var horse = GetRoomUserByVirtualId(user.HorseId);
                            if (horse != null)
                            {
                                horse.X = user.SetX;
                                horse.Y = user.SetY;
                            }
                        }
                        if (user.X == _room.GetGameMap().Model.DoorX && user.Y == _room.GetGameMap().Model.DoorY && !toRemove.Contains(user) && !user.IsBot)
                        {
                            toRemove.Add(user);
                            continue;
                        }
                        var items = _room.GetGameMap().GetCoordinatedItems(new(user.X, user.Y));
                        foreach (var item in items.ToList()) item.UserWalksOnFurni(user);
                        UpdateUserStatus(user, true);
                    }
                    else
                        invalidStep = true;
                    user.SetStep = false;
                }
                if (user.PathRecalcNeeded)
                {
                    if (user.Path.Count > 1)
                        user.Path.Clear();
                    user.Path = PathFinder.FindPath(user, _room.GetGameMap().DiagonalEnabled, _room.GetGameMap(), new(user.X, user.Y), new(user.GoalX, user.GoalY));
                    if (user.Path.Count > 1)
                    {
                        user.PathStep = 1;
                        user.IsWalking = true;
                        user.PathRecalcNeeded = false;
                    }
                    else
                    {
                        user.PathRecalcNeeded = false;
                        if (user.Path.Count > 1)
                            user.Path.Clear();
                    }
                }
                if (user.IsWalking && !user.Freezed)
                {
                    if (invalidStep || user.PathStep >= user.Path.Count || user.GoalX == user.X && user.GoalY == user.Y) //No path found, or reached goal (:
                    {
                        user.IsWalking = false;
                        user.RemoveStatus("mv");
                        if (user.Statusses.ContainsKey("sign"))
                            user.RemoveStatus("sign");
                        if (user.IsBot && user.BotData.TargetUser > 0)
                        {
                            if (user.CarryItemId > 0)
                            {
                                var target = _room.GetRoomUserManager().GetRoomUserByHabbo(user.BotData.TargetUser);
                                if (target != null && Gamemap.TilesTouching(user.X, user.Y, target.X, target.Y))
                                {
                                    user.SetRot(Rotation.Calculate(user.X, user.Y, target.X, target.Y), false);
                                    target.SetRot(Rotation.Calculate(target.X, target.Y, user.X, user.Y), false);
                                    target.CarryItem(user.CarryItemId);
                                }
                            }
                            user.CarryItem(0);
                            user.BotData.TargetUser = 0;
                        }
                        if (user.RidingHorse && user.IsPet == false && !user.IsBot)
                        {
                            var mascotaVinculada = GetRoomUserByVirtualId(user.HorseId);
                            if (mascotaVinculada != null)
                            {
                                mascotaVinculada.IsWalking = false;
                                mascotaVinculada.RemoveStatus("mv");
                                mascotaVinculada.UpdateNeeded = true;
                            }
                        }
                    }
                    else
                    {
                        var nextStep = user.Path[user.Path.Count - user.PathStep - 1];
                        user.PathStep++;
                        if (user.FastWalking && user.PathStep < user.Path.Count)
                        {
                            var s2 = user.Path.Count - user.PathStep - 1;
                            nextStep = user.Path[s2];
                            user.PathStep++;
                        }
                        if (user.SuperFastWalking && user.PathStep < user.Path.Count)
                        {
                            var s2 = user.Path.Count - user.PathStep - 1;
                            nextStep = user.Path[s2];
                            user.PathStep++;
                            user.PathStep++;
                        }
                        var nextX = nextStep.X;
                        var nextY = nextStep.Y;
                        user.RemoveStatus("mv");
                        if (_room.GetGameMap().IsValidStep2(user, new(user.X, user.Y), new(nextX, nextY), user.GoalX == nextX && user.GoalY == nextY, user.AllowOverride))
                        {
                            var nextZ = _room.GetGameMap().SqAbsoluteHeight(nextX, nextY);
                            if (!user.IsBot)
                            {
                                if (user.IsSitting)
                                {
                                    user.Statusses.Remove("sit");
                                    user.Z += 0.35;
                                    user.IsSitting = false;
                                    user.UpdateNeeded = true;
                                }
                                else if (user.IsLying)
                                {
                                    user.Statusses.Remove("sit");
                                    user.Z += 0.35;
                                    user.IsLying = false;
                                    user.UpdateNeeded = true;
                                }
                            }
                            if (!user.IsBot)
                            {
                                user.Statusses.Remove("lay");
                                user.Statusses.Remove("sit");
                            }
                            if (!user.IsBot && !user.IsPet && user.GetClient() != null)
                            {
                                if (user.GetClient().GetHabbo().IsTeleporting)
                                {
                                    user.GetClient().GetHabbo().IsTeleporting = false;
                                    user.GetClient().GetHabbo().TeleporterId = 0;
                                }
                                else if (user.GetClient().GetHabbo().IsHopping)
                                {
                                    user.GetClient().GetHabbo().IsHopping = false;
                                    user.GetClient().GetHabbo().HopperId = 0;
                                }
                            }
                            if (!user.IsBot && user.RidingHorse && user.IsPet == false)
                            {
                                var horse = GetRoomUserByVirtualId(user.HorseId);
                                if (horse != null)
                                    horse.SetStatus("mv", $"{nextX},{nextY},{TextHandling.GetString(nextZ)}");
                                user.SetStatus("mv", $"{+nextX},{nextY},{TextHandling.GetString(nextZ + 1)}");
                                user.UpdateNeeded = true;
                                horse.UpdateNeeded = true;
                            }
                            else
                                user.SetStatus("mv", $"{nextX},{nextY},{TextHandling.GetString(nextZ)}");
                            var newRot = Rotation.Calculate(user.X, user.Y, nextX, nextY, user.MoonwalkEnabled);
                            user.RotBody = newRot;
                            user.RotHead = newRot;
                            user.SetStep = true;
                            user.SetX = nextX;
                            user.SetY = nextY;
                            user.SetZ = nextZ;
                            UpdateUserEffect(user, user.SetX, user.SetY);
                            updated = true;
                            if (user.RidingHorse && user.IsPet == false && !user.IsBot)
                            {
                                var horse = GetRoomUserByVirtualId(user.HorseId);
                                if (horse != null)
                                {
                                    horse.RotBody = newRot;
                                    horse.RotHead = newRot;
                                    horse.SetStep = true;
                                    horse.SetX = nextX;
                                    horse.SetY = nextY;
                                    horse.SetZ = nextZ;
                                }
                            }
                            _room.GetGameMap().GameMap[user.X, user.Y] = user.SqState; // REstore the old one
                            user.SqState = _room.GetGameMap().GameMap[user.SetX, user.SetY]; //Backup the new one
                            if (!_room.RoomBlockingEnabled)
                            {
                                var users = _room.GetRoomUserManager().GetUserForSquare(nextX, nextY);
                                if (users != null)
                                    _room.GetGameMap().GameMap[nextX, nextY] = 0;
                            }
                            else
                                _room.GetGameMap().GameMap[nextX, nextY] = 1;
                        }
                    }
                    if (!user.RidingHorse)
                        user.UpdateNeeded = true;
                }
                else
                {
                    if (user.Statusses.ContainsKey("mv"))
                    {
                        user.RemoveStatus("mv");
                        user.UpdateNeeded = true;
                        if (user.RidingHorse)
                        {
                            var horse = GetRoomUserByVirtualId(user.HorseId);
                            if (horse != null)
                            {
                                horse.RemoveStatus("mv");
                                horse.UpdateNeeded = true;
                            }
                        }
                    }
                }
                if (user.RidingHorse)
                    user.ApplyEffect(77);
                if (user.IsBot && user.BotAi != null)
                    user.BotAi.OnTimerTick();
                else
                    userCounter++;
                if (!updated) UpdateUserEffect(user, user.X, user.Y);
            }
            foreach (var userToRemove in toRemove.ToList())
            {
                var client = PlusEnvironment.Game.ClientManager.GetClientByUserId(userToRemove.HabboId);
                if (client != null)
                    RemoveUserFromRoom(client, true);
                else
                    RemoveRoomUser(userToRemove);
            }
            if (UserCount != userCounter)
                UpdateUserCount(userCounter);
        }
        catch (Exception e)
        {
            ExceptionLogger.LogCriticalException(e);
        }
    }

    public void UpdateUserStatus(RoomUser user, bool cyclegameitems)
    {
        if (user == null)
            return;
        try
        {
            var isBot = user.IsBot;
            if (isBot)
                cyclegameitems = false;
            if (UnixTimestamp.GetNow() > UnixTimestamp.GetNow() + user.SignTime)
            {
                if (user.Statusses.ContainsKey("sign"))
                {
                    user.Statusses.Remove("sign");
                    user.UpdateNeeded = true;
                }
            }
            if (user.Statusses.ContainsKey("lay") && !user.IsLying || user.Statusses.ContainsKey("sit") && !user.IsSitting)
            {
                if (user.Statusses.ContainsKey("lay"))
                    user.Statusses.Remove("lay");
                if (user.Statusses.ContainsKey("sit"))
                    user.Statusses.Remove("sit");
                user.UpdateNeeded = true;
            }
            else if (user.IsLying || user.IsSitting)
                return;
            double newZ;
            var itemsOnSquare = _room.GetGameMap().GetAllRoomItemForSquare(user.X, user.Y);
            if (itemsOnSquare != null || itemsOnSquare.Count != 0)
            {
                if (user.RidingHorse && user.IsPet == false)
                    newZ = _room.GetGameMap().SqAbsoluteHeight(user.X, user.Y, itemsOnSquare.ToList()) + 1;
                else
                    newZ = _room.GetGameMap().SqAbsoluteHeight(user.X, user.Y, itemsOnSquare.ToList());
            }
            else
                newZ = 1;
            if (newZ != user.Z)
            {
                user.Z = newZ;
                user.UpdateNeeded = true;
            }
            var model = _room.GetGameMap().Model;
            if (model.SqState[user.X, user.Y] == SquareState.Seat)
            {
                if (!user.Statusses.ContainsKey("sit"))
                    user.Statusses.Add("sit", "1.0");
                user.Z = model.SqFloorHeight[user.X, user.Y];
                user.RotHead = model.SqSeatRot[user.X, user.Y];
                user.RotBody = model.SqSeatRot[user.X, user.Y];
                user.UpdateNeeded = true;
            }
            if (itemsOnSquare.Count == 0)
                user.LastItem = null;
            foreach (var item in itemsOnSquare.ToList())
            {
                if (item == null)
                    continue;
                if (item.Definition.IsSeat)
                {
                    if (!user.Statusses.ContainsKey("sit"))
                    {
                        if (!user.Statusses.ContainsKey("sit"))
                            user.Statusses.Add("sit", TextHandling.GetString(item.Definition.Height));
                    }
                    user.Z = item.GetZ;
                    user.RotHead = item.Rotation;
                    user.RotBody = item.Rotation;
                    user.UpdateNeeded = true;
                }
                switch (item.Definition.InteractionType)
                {
                    case InteractionType.Bed:
                    case InteractionType.TentSmall:
                        {
                            if (!user.Statusses.ContainsKey("lay"))
                                user.Statusses.Add("lay", $"{TextHandling.GetString(item.Definition.Height)} null");
                            user.Z = item.GetZ;
                            user.RotHead = item.Rotation;
                            user.RotBody = item.Rotation;
                            user.UpdateNeeded = true;
                            break;
                        }
                    case InteractionType.Banzaigategreen:
                    case InteractionType.Banzaigateblue:
                    case InteractionType.Banzaigatered:
                    case InteractionType.Banzaigateyellow:
                        {
                            if (cyclegameitems)
                            {
                                var effectId = Convert.ToInt32(item.Team + 32);
                                var t = user.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForBanzai();
                                if (user.Team == Team.None)
                                {
                                    if (t.CanEnterOnTeam(item.Team))
                                    {
                                        if (user.Team != Team.None)
                                            t.OnUserLeave(user);
                                        user.Team = item.Team;
                                        t.AddUser(user);
                                        if (user.GetClient().GetHabbo().Effects.CurrentEffect != effectId)
                                            user.GetClient().GetHabbo().Effects.ApplyEffect(effectId);
                                    }
                                }
                                else if (user.Team != Team.None && user.Team != item.Team)
                                {
                                    t.OnUserLeave(user);
                                    user.Team = Team.None;
                                    user.GetClient().GetHabbo().Effects.ApplyEffect(0);
                                }
                                else
                                {
                                    //usersOnTeam--;
                                    t.OnUserLeave(user);
                                    if (user.GetClient().GetHabbo().Effects.CurrentEffect == effectId)
                                        user.GetClient().GetHabbo().Effects.ApplyEffect(0);
                                    user.Team = Team.None;
                                }
                                //Item.ExtraData = usersOnTeam.ToString();
                                //Item.UpdateState(false, true);
                            }
                            break;
                        }
                    case InteractionType.FreezeYellowGate:
                    case InteractionType.FreezeRedGate:
                    case InteractionType.FreezeGreenGate:
                    case InteractionType.FreezeBlueGate:
                        {
                            if (cyclegameitems)
                            {
                                var effectId = Convert.ToInt32(item.Team + 39);
                                var t = user.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForFreeze();
                                if (user.Team == Team.None)
                                {
                                    if (t.CanEnterOnTeam(item.Team))
                                    {
                                        if (user.Team != Team.None)
                                            t.OnUserLeave(user);
                                        user.Team = item.Team;
                                        t.AddUser(user);
                                        if (user.GetClient().GetHabbo().Effects.CurrentEffect != effectId)
                                            user.GetClient().GetHabbo().Effects.ApplyEffect(effectId);
                                    }
                                }
                                else if (user.Team != Team.None && user.Team != item.Team)
                                {
                                    t.OnUserLeave(user);
                                    user.Team = Team.None;
                                    user.GetClient().GetHabbo().Effects.ApplyEffect(0);
                                }
                                else
                                {
                                    //usersOnTeam--;
                                    t.OnUserLeave(user);
                                    if (user.GetClient().GetHabbo().Effects.CurrentEffect == effectId)
                                        user.GetClient().GetHabbo().Effects.ApplyEffect(0);
                                    user.Team = Team.None;
                                }
                                //Item.ExtraData = usersOnTeam.ToString();
                                //Item.UpdateState(false, true);
                            }
                            break;
                        }
                    case InteractionType.Banzaitele:
                        {
                            if (user.Statusses.ContainsKey("mv"))
                                _room.GetGameItemHandler().OnTeleportRoomUserEnter(user, item);
                            break;
                        }
                    case InteractionType.Effect:
                        {
                            if (user == null)
                                return;
                            if (!user.IsBot)
                            {
                                if (item == null || item.Definition == null || user.GetClient() == null || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().Effects == null)
                                    return;
                                if (item.Definition.EffectId == 0 && user.GetClient().GetHabbo().Effects.CurrentEffect == 0)
                                    return;
                                user.GetClient().GetHabbo().Effects.ApplyEffect(item.Definition.EffectId);
                                item.LegacyDataString = "1";
                                item.UpdateState(false, true);
                                item.RequestUpdate(2, true);
                            }
                            break;
                        }
                    case InteractionType.Arrow:
                        {
                            if (user.GoalX == item.GetX && user.GoalY == item.GetY)
                            {
                                if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                                    continue;
                                var room = user.GetClient().GetHabbo().CurrentRoom;
                                if (room == null)
                                    return;
                                if (!ItemTeleporterFinder.IsTeleLinked(item.Id, room))
                                    user.UnlockWalking();
                                else
                                {
                                    var linkedTele = ItemTeleporterFinder.GetLinkedTele(item.Id);
                                    var teleRoomId = ItemTeleporterFinder.GetTeleRoomId(linkedTele, room);
                                    if (teleRoomId == room.RoomId)
                                    {
                                        var targetItem = room.GetRoomItemHandler().GetItem(linkedTele);
                                        if (targetItem == null)
                                        {
                                            if (user.GetClient() != null)
                                                user.GetClient().SendWhisper("Hey, that arrow is poorly!");
                                            return;
                                        }
                                        room.GetGameMap().TeleportToItem(user, targetItem);
                                    }
                                    else if (teleRoomId != room.RoomId)
                                    {
                                        if (user != null && !user.IsBot && user.GetClient() != null && user.GetClient().GetHabbo() != null)
                                        {
                                            user.GetClient().GetHabbo().IsTeleporting = true;
                                            user.GetClient().GetHabbo().TeleportingRoomId = teleRoomId;
                                            user.GetClient().GetHabbo().TeleporterId = linkedTele;
                                            user.GetClient().GetHabbo().PrepareRoom(teleRoomId, "");
                                        }
                                    }
                                    else if (_room.GetRoomItemHandler().GetItem(linkedTele) != null)
                                    {
                                        user.SetPos(item.GetX, item.GetY, item.GetZ);
                                        user.SetRot(item.Rotation, false);
                                    }
                                    else
                                        user.UnlockWalking();
                                }
                            }
                            break;
                        }
                }
            }
            if (user.IsSitting && user.TeleportEnabled)
            {
                user.Z -= 0.35;
                user.UpdateNeeded = true;
            }
            if (cyclegameitems)
            {
                if (_room.GotSoccer())
                    _room.GetSoccer().OnUserWalk(user);
                if (_room.GotBanzai())
                    _room.GetBanzai().OnUserWalk(user);
                if (_room.GotFreeze())
                    _room.GetFreeze().OnUserWalk(user);
            }
        }
        catch (Exception e)
        {
            ExceptionLogger.LogException(e);
        }
    }

    private void UpdateUserEffect(RoomUser user, int x, int y)
    {
        if (user == null || user.IsBot || user.GetClient() == null || user.GetClient().GetHabbo() == null)
            return;
        try
        {
            var newCurrentUserItemEffect = _room.GetGameMap().EffectMap[x, y];
            if (newCurrentUserItemEffect > 0)
            {
                if (user.GetClient().GetHabbo().Effects.CurrentEffect == 0)
                    user.CurrentItemEffect = ItemEffectType.None;
                var type = ByteToItemEffectEnum.Parse(newCurrentUserItemEffect);
                if (type != user.CurrentItemEffect)
                {
                    switch (type)
                    {
                        case ItemEffectType.Iceskates:
                            {
                                user.GetClient().GetHabbo().Effects.ApplyEffect(user.GetClient().GetHabbo().Gender == "M" ? 38 : 39);
                                user.CurrentItemEffect = ItemEffectType.Iceskates;
                                break;
                            }
                        case ItemEffectType.Normalskates:
                            {
                                user.GetClient().GetHabbo().Effects.ApplyEffect(user.GetClient().GetHabbo().Gender == "M" ? 55 : 56);
                                user.CurrentItemEffect = type;
                                break;
                            }
                        case ItemEffectType.Swim:
                            {
                                user.GetClient().GetHabbo().Effects.ApplyEffect(29);
                                user.CurrentItemEffect = type;
                                break;
                            }
                        case ItemEffectType.SwimLow:
                            {
                                user.GetClient().GetHabbo().Effects.ApplyEffect(30);
                                user.CurrentItemEffect = type;
                                break;
                            }
                        case ItemEffectType.SwimHalloween:
                            {
                                user.GetClient().GetHabbo().Effects.ApplyEffect(37);
                                user.CurrentItemEffect = type;
                                break;
                            }
                        case ItemEffectType.None:
                            {
                                user.GetClient().GetHabbo().Effects.ApplyEffect(-1);
                                user.CurrentItemEffect = type;
                                break;
                            }
                    }
                }
            }
            else if (user.CurrentItemEffect != ItemEffectType.None && newCurrentUserItemEffect == 0)
            {
                user.GetClient().GetHabbo().Effects.ApplyEffect(-1);
                user.CurrentItemEffect = ItemEffectType.None;
            }
        }
        catch { }
    }

    public ICollection<RoomUser> GetUserList() => _users.Values;

    public void Dispose()
    {
        UpdatePets();
        UpdateBots();
        _room.UsersNow = 0;
        using (var dbClient = PlusEnvironment.DatabaseManager.GetQueryReactor())
        {
            dbClient.RunQuery($"UPDATE `rooms` SET `users_now` = '0' WHERE `id` = '{_room.Id}' LIMIT 1");
        }
        _users.Clear();
        _pets.Clear();
        _bots.Clear();
        UserCount = 0;
        PetCount = 0;
        _users = null;
        _pets = null;
        _bots = null;
        _room = null;
    }
}
using System.Linq;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.FloorPlan;

internal class SaveFloorPlanModelEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IDatabase _database;

    public SaveFloorPlanModelEvent(IRoomManager roomManager, IDatabase database)
    {
        _roomManager = roomManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var room = session.GetHabbo().CurrentRoom;
        if (room == null || session.GetHabbo().CurrentRoomId != room.Id || !room.CheckRights(session, true))
            return Task.CompletedTask;
        char[] validLetters =
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g',
            'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', '\r'
        };
        var map = packet.PopString().ToLower().TrimEnd();
        if (map.Length > 4159) //4096 + New Lines = 4159
        {
            session.SendPacket(new RoomNotificationComposer("floorplan_editor.error", "errors", "(%%%general%%%): %%%too_large_area%%% (%%%max%%% 2048 %%%tiles%%%)"));
            return Task.CompletedTask;
        }
        if (map.Any(letter => !validLetters.Contains(letter)) || string.IsNullOrEmpty(map))
        {
            session.SendPacket(new RoomNotificationComposer("floorplan_editor.error", "errors", "Oops, it appears that you have entered an invalid floor map!"));
            return Task.CompletedTask;
        }
        var modelData = map.Split('\r');
        var sizeY = modelData.Length;
        var sizeX = modelData[0].Length;
        if (sizeY > 64 || sizeX > 64)
        {
            session.SendPacket(new RoomNotificationComposer("floorplan_editor.error", "errors", "The maximum height and width of a model is 64x64!"));
            return Task.CompletedTask;
        }
        var lastLineLength = 0;
        var isValid = true;
        foreach (var data in modelData)
        {
            if (lastLineLength == 0)
            {
                lastLineLength = data.Length;
                continue;
            }
            if (lastLineLength != data.Length) isValid = false;
        }
        if (!isValid)
        {
            session.SendPacket(new RoomNotificationComposer("floorplan_editor.error", "errors", "Oops, it appears that you have entered an invalid floor map!"));
            return Task.CompletedTask;
        }
        var doorX = packet.PopInt();
        var doorY = packet.PopInt();
        var doorDirection = packet.PopInt();
        var wallThick = packet.PopInt();
        var floorThick = packet.PopInt();
        var wallHeight = packet.PopInt();
        var doorZ = 0;
        try
        {
            doorZ = Parse(modelData[doorY][doorX]);
        }
        catch
        {
            //ignored
        }
        if (wallThick > 1)
            wallThick = 1;
        if (wallThick < -2)
            wallThick = -2;
        if (floorThick > 1)
            floorThick = 1;
        if (floorThick < -2)
            wallThick = -2;
        if (wallHeight < 0)
            wallHeight = 0;
        if (wallHeight > 15)
            wallHeight = 15;
        var modelName = "model_bc_" + room.Id;
        map += '\r' + new string('x', sizeX);
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT * FROM `room_models` WHERE `id` = @model AND `custom` = '1' LIMIT 1");
            dbClient.AddParameter("model", "model_bc_" + room.Id);
            var row = dbClient.GetRow();
            if (row == null) //The row is still null, let's insert instead.
            {
                dbClient.SetQuery(
                    "INSERT INTO `room_models` (`id`,`door_x`,`door_y`, `door_z`, `door_dir`,`heightmap`,`custom`,`wall_height`) VALUES (@ModelName, @DoorX, @DoorY, @DoorZ, @DoorDirection, @Map,'1',@WallHeight)");
                dbClient.AddParameter("ModelName", "model_bc_" + room.Id);
                dbClient.AddParameter("DoorX", doorX);
                dbClient.AddParameter("DoorY", doorY);
                dbClient.AddParameter("DoorDirection", doorDirection);
                dbClient.AddParameter("DoorZ", doorZ);
                dbClient.AddParameter("Map", map);
                dbClient.AddParameter("WallHeight", wallHeight);
                dbClient.RunQuery();
            }
            else
            {
                dbClient.SetQuery(
                    "UPDATE `room_models` SET `heightmap` = @Map, `door_x` = @DoorX, `door_y` = @DoorY, `door_z` = @DoorZ,  `door_dir` = @DoorDirection, `wall_height` = @WallHeight WHERE `id` = @ModelName LIMIT 1");
                dbClient.AddParameter("ModelName", "model_bc_" + room.Id);
                dbClient.AddParameter("Map", map);
                dbClient.AddParameter("DoorX", doorX);
                dbClient.AddParameter("DoorY", doorY);
                dbClient.AddParameter("DoorZ", doorZ);
                dbClient.AddParameter("DoorDirection", doorDirection);
                dbClient.AddParameter("WallHeight", wallHeight);
                dbClient.RunQuery();
            }
            dbClient.SetQuery("UPDATE `rooms` SET `model_name` = @ModelName, `wallthick` = @WallThick, `floorthick` = @FloorThick WHERE `id` = @roomId LIMIT 1");
            dbClient.AddParameter("roomId", room.Id);
            dbClient.AddParameter("ModelName", "model_bc_" + room.Id);
            dbClient.AddParameter("WallThick", wallThick);
            dbClient.AddParameter("FloorThick", floorThick);
            dbClient.RunQuery();
        }
        room.ModelName = modelName;
        room.WallThickness = wallThick;
        room.FloorThickness = floorThick;
        var usersToReturn = room.GetRoomUserManager().GetRoomUsers().ToList();
        _roomManager.ReloadModel(modelName);
        _roomManager.UnloadRoom(room.Id);
        foreach (var user in usersToReturn)
        {
            if (user == null || user.GetClient() == null)
                continue;
            user.GetClient().SendPacket(new RoomForwardComposer(room.Id));
        }
        return Task.CompletedTask;
    }

    private static short Parse(char input)
    {
        switch (input)
        {
            default:
                return 0;
            case '1':
                return 1;
            case '2':
                return 2;
            case '3':
                return 3;
            case '4':
                return 4;
            case '5':
                return 5;
            case '6':
                return 6;
            case '7':
                return 7;
            case '8':
                return 8;
            case '9':
                return 9;
            case 'a':
                return 10;
            case 'b':
                return 11;
            case 'c':
                return 12;
            case 'd':
                return 13;
            case 'e':
                return 14;
            case 'f':
                return 15;
            case 'g':
                return 16;
            case 'h':
                return 17;
            case 'i':
                return 18;
            case 'j':
                return 19;
            case 'k':
                return 20;
            case 'l':
                return 21;
            case 'm':
                return 22;
            case 'n':
                return 23;
            case 'o':
                return 24;
            case 'p':
                return 25;
            case 'q':
                return 26;
            case 'r':
                return 27;
            case 's':
                return 28;
            case 't':
                return 29;
            case 'u':
                return 30;
            case 'v':
                return 31;
            case 'w':
                return 32;
        }
    }
}
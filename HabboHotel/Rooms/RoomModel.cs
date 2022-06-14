namespace Plus.HabboHotel.Rooms;

public class RoomModel
{
    public bool ClubOnly;
    public int DoorOrientation;
    public int DoorX;
    public int DoorY;
    public double DoorZ;
    public string Heightmap;
    public int MapSizeX;
    public int MapSizeY;

    public byte[,] MRoomModelfx;
    public short[,] SqFloorHeight;
    public byte[,] SqSeatRot;
    public SquareState[,] SqState;

    public string StaticFurniMap;

    public int WallHeight;

    public RoomModel(string id, int doorX, int doorY, double doorZ, int doorOrientation, string heightmap, bool clubOnly, int wallHeight, bool custom)
    {
        try
        {
            Id = id;
            DoorX = doorX;
            DoorY = doorY;
            DoorZ = doorZ;
            DoorOrientation = doorOrientation;
            WallHeight = wallHeight;
            Heightmap = heightmap.ToLower();
            var tmpHeightmap = heightmap.Split(Convert.ToChar(13));
            MapSizeX = tmpHeightmap[0].Length;
            MapSizeY = tmpHeightmap.Length;
            ClubOnly = clubOnly;
            SqState = new SquareState[MapSizeX, MapSizeY];
            SqFloorHeight = new short[MapSizeX, MapSizeY];
            SqSeatRot = new byte[MapSizeX, MapSizeY];
            for (var y = 0; y < MapSizeY; y++)
            {
                var line = tmpHeightmap[y];
                line = line.Replace("\r", "");
                line = line.Replace("\n", "");
                var x = 0;
                foreach (var square in line)
                {
                    if (square == 'x')
                        SqState[x, y] = SquareState.Blocked;
                    else
                    {
                        SqState[x, y] = SquareState.Open;
                        SqFloorHeight[x, y] = Parse(square);
                    }
                    x++;
                }
            }
        }
        catch //(Exception e)
        {
            //Console.WriteLine("Note: " + id + " failed to load properly.");
            //Console.WriteLine("Error during room modeldata loading for model " + Heightmap);
            //throw e;
        }
    }

    public string Id { get; set; }

    public static short Parse(char input)
    {
        switch (input)
        {
            case '0':
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
            default:
                throw new FormatException("The input was not in a correct format: input must be between (0-k)");
        }
    }

    public void Destroy()
    {
        Heightmap = null;
        SqState = null;
        SqFloorHeight = null;
        SqSeatRot = null;
    }
}
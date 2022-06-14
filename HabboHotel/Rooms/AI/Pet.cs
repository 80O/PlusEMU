using Plus.Communication.Packets.Outgoing.Pets;
using Plus.Communication.Packets.Outgoing.Rooms.AI.Pets;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Utilities;

namespace Plus.HabboHotel.Rooms.AI;

public class Pet
{
    public int AnyoneCanRide;
    public string Color;
    public double CreationStamp;
    public PetDatabaseUpdateState DbState;

    public int Energy;
    public int Experience;

    public int[] ExperienceLevels = { 100, 200, 400, 600, 1000, 1300, 1800, 2400, 3200, 4300, 7200, 8500, 10100, 13300, 17500, 23000, 51900, 75000, 128000, 150000 };

    public string GnomeClothing;
    public int HairDye;
    public string Name;
    public int Nutrition;
    public int OwnerId;
    public int PetHair;
    public int PetId;
    public bool PlacedInRoom;
    public string Race;
    public int Respect;
    public int RoomId;
    public int Saddle;

    public int Type;
    public int VirtualId;
    public int X;
    public int Y;
    public double Z;

    public Pet(int petId, int ownerId, int roomId, string name, int type, string race, string color, int experience, int energy, int nutrition, int respect, double creationStamp, int x, int y,
        double z, int saddle, int anyonecanride, int dye, int petHer, string gnomeClothing)
    {
        PetId = petId;
        OwnerId = ownerId;
        RoomId = roomId;
        Name = name;
        Type = type;
        Race = race;
        Color = color;
        Experience = experience;
        Energy = energy;
        Nutrition = nutrition;
        Respect = respect;
        CreationStamp = creationStamp;
        X = x;
        Y = y;
        Z = z;
        PlacedInRoom = false;
        DbState = PetDatabaseUpdateState.Updated;
        Saddle = saddle;
        AnyoneCanRide = anyonecanride;
        PetHair = petHer;
        HairDye = dye;
        GnomeClothing = gnomeClothing;

        /// TODO: pass by constructor
        OwnerName = PlusEnvironment.GetGame().GetClientManager().GetNameById(OwnerId).Result;
    }

    public Room Room
    {
        get
        {
            if (!IsInRoom)
                return null;
            if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(RoomId, out var room))
                return room;
            return null;
        }
    }

    public bool IsInRoom => RoomId > 0;

    public int Level
    {
        get
        {
            for (var level = 0; level < ExperienceLevels.Length; ++level)
            {
                if (Experience < ExperienceLevels[level])
                    return level + 1;
            }
            return ExperienceLevels.Length;
        }
    }

    public static int MaxLevel => 20;

    public int ExperienceGoal =>
        //will error index out of range (need to look into this sometime)
        ExperienceLevels[Level - 1];

    public static int MaxEnergy => 100;

    public static int MaxNutrition => 150;

    public int Age => Convert.ToInt32(Math.Floor((UnixTimestamp.GetNow() - CreationStamp) / 86400));

    public string Look => Type + " " + Race + " " + Color + " " + GnomeClothing;

    public string OwnerName { get; set; }

    public void OnRespect()
    {
        Respect++;
        Room.SendPacket(new RespectPetNotificationMessageComposer(this));
        if (DbState != PetDatabaseUpdateState.NeedsInsert)
            DbState = PetDatabaseUpdateState.NeedsUpdate;
        if (Experience <= 150000)
            Addexperience(10);
    }

    public void Addexperience(int amount)
    {
        Experience = Experience + amount;
        if (Experience > 150000)
        {
            Experience = 150000;
            if (Room != null)
                Room.SendPacket(new AddExperiencePointsComposer(PetId, VirtualId, amount));
            return;
        }
        if (DbState != PetDatabaseUpdateState.NeedsInsert)
            DbState = PetDatabaseUpdateState.NeedsUpdate;
        if (Room != null)
        {
            Room.SendPacket(new AddExperiencePointsComposer(PetId, VirtualId, amount));
            if (Experience >= ExperienceGoal)
                Room.SendPacket(new ChatComposer(VirtualId, "*leveled up to level " + Level + " *", 0, 0));
        }
    }

    public void PetEnergy(bool add)
    {
        int maxE;
        if (add)
        {
            if (Energy == 100) // If Energy is 100, no point.
                return;
            if (Energy > 85)
                maxE = MaxEnergy - Energy;
            else
                maxE = 10;
        }
        else
            maxE = 15; // Remove Max Energy as 15
        if (maxE <= 4)
            maxE = 15;
        var r = Random.Shared.Next(4, maxE + 1);
        if (!add)
        {
            Energy = Energy - r;
            if (Energy < 0)
            {
                Energy = 1;
                r = 1;
            }
        }
        else
            Energy = Energy + r;
        if (DbState != PetDatabaseUpdateState.NeedsInsert)
            DbState = PetDatabaseUpdateState.NeedsUpdate;
    }
}
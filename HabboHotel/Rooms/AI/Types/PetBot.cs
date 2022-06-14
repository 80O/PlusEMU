using Plus.Core;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.PathFinding;
using Plus.Utilities;

namespace Plus.HabboHotel.Rooms.AI.Types;

public class PetBot : BotAi
{
    private int _actionTimer;
    private int _energyTimer;
    private int _speechTimer;

    public PetBot(int virtualId)
    {
        _speechTimer = Random.Shared.Next(10, 60);
        _actionTimer = Random.Shared.Next(10, 30 + virtualId);
        _energyTimer = Random.Shared.Next(10, 60);
    }

    private void RemovePetStatus()
    {
        var pet = GetRoomUser();
        if (pet != null)
        {
            foreach (var kvp in pet.Statusses.ToList())
            {
                if (pet.Statusses.ContainsKey(kvp.Key))
                    pet.Statusses.Remove(kvp.Key);
            }
        }
    }

    public override void OnSelfEnterRoom()
    {
        var nextCoord = GetRoom().GetGameMap().GetRandomWalkableSquare();
        //int randomX = PlusEnvironment.GetRandomNumber(0, GetRoom().Model.MapSizeX);
        //int randomY = PlusEnvironment.GetRandomNumber(0, GetRoom().Model.MapSizeY);
        if (GetRoomUser() != null)
            GetRoomUser().MoveTo(nextCoord.X, nextCoord.Y);
    }

    public override void OnSelfLeaveRoom(bool kicked) { }


    public override void OnUserEnterRoom(RoomUser user)
    {
        if (user.GetClient() != null && user.GetClient().GetHabbo() != null)
        {
            var pet = GetRoomUser();
            if (pet != null)
            {
                if (user.GetClient().GetHabbo().Username == pet.PetData.OwnerName)
                {
                    var speech = PlusEnvironment.GetGame().GetChatManager().GetPetLocale().GetValue("welcome.speech.pet" + pet.PetData.Type);
                    var rSpeech = speech[Random.Shared.Next(0, speech.Length)];
                    pet.Chat(rSpeech);
                }
            }
        }
    }

    public override void OnUserLeaveRoom(GameClient client) { }

    public override void OnUserShout(RoomUser user, string message) { }

    public override void OnTimerTick()
    {
        var pet = GetRoomUser();
        if (pet == null)
            return;
        if (_speechTimer <= 0)
        {
            if (pet.PetData.DbState != PetDatabaseUpdateState.NeedsInsert)
                pet.PetData.DbState = PetDatabaseUpdateState.NeedsUpdate;
            if (pet != null)
            {
                RemovePetStatus();
                var speech = PlusEnvironment.GetGame().GetChatManager().GetPetLocale().GetValue("speech.pet" + pet.PetData.Type);
                var rSpeech = speech[Random.Shared.Next(0, speech.Length)];
                if (rSpeech.Length != 3)
                    pet.Chat(rSpeech);
                else
                    pet.Statusses.Add(rSpeech, TextHandling.GetString(pet.Z));
            }
            _speechTimer = Random.Shared.Next(20, 120 + 1);
        }
        else
            _speechTimer--;
        if (_actionTimer <= 0)
        {
            try
            {
                RemovePetStatus();
                _actionTimer = Random.Shared.Next(15, 40 + GetRoomUser().PetData.VirtualId + 1);
                if (!GetRoomUser().RidingHorse)
                {
                    // Remove Status
                    RemovePetStatus();
                    var nextCoord = GetRoom().GetGameMap().GetRandomWalkableSquare();
                    if (GetRoomUser().CanWalk)
                        GetRoomUser().MoveTo(nextCoord.X, nextCoord.Y);
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }
        else
            _actionTimer--;
        if (_energyTimer <= 0)
        {
            RemovePetStatus(); // Remove Status
            pet.PetData.PetEnergy(true); // Add Energy
            _energyTimer = Random.Shared.Next(30, 120 + 1); // 2 Min Max
        }
        else
            _energyTimer--;
    }

    public override void OnUserSay(RoomUser user, string message)
    {
        if (user == null)
            return;
        var pet = GetRoomUser();
        if (pet == null)
            return;
        if (pet.PetData.DbState != PetDatabaseUpdateState.NeedsInsert)
            pet.PetData.DbState = PetDatabaseUpdateState.NeedsUpdate;
        if (message.ToLower().Equals(pet.PetData.Name.ToLower()))
        {
            pet.SetRot(Rotation.Calculate(pet.X, pet.Y, user.X, user.Y), false);
            return;
        }

        //if (!Pet.Statusses.ContainsKey("gst thr"))
        //    Pet.Statusses.Add("gst thr", TextHandling.GetString(Pet.Z));
        if (message.ToLower().StartsWith(pet.PetData.Name.ToLower() + " ") && user.GetClient().GetHabbo().Username.ToLower() == pet.PetData.OwnerName.ToLower() ||
            message.ToLower().StartsWith(pet.PetData.Name.ToLower() + " ") &&
            PlusEnvironment.GetGame().GetChatManager().GetPetCommands().TryInvoke(message.Substring(pet.PetData.Name.ToLower().Length + 1)) == 8)
        {
            var command = message.Substring(pet.PetData.Name.ToLower().Length + 1);
            var r = Random.Shared.Next(1, 8 + 1); // Made Random
            if (pet.PetData.Energy > 10 && r < 6 || pet.PetData.Level > 15 || PlusEnvironment.GetGame().GetChatManager().GetPetCommands().TryInvoke(command) == 8)
            {
                RemovePetStatus(); // Remove Status
                switch (PlusEnvironment.GetGame().GetChatManager().GetPetCommands().TryInvoke(command))
                {
                    // TODO - Level you can use the commands at...
                    case 1:
                        RemovePetStatus();

                        //int randomX = PlusEnvironment.GetRandomNumber(0, GetRoom().Model.MapSizeX);
                        //int randomY = PlusEnvironment.GetRandomNumber(0, GetRoom().Model.MapSizeY);
                        var nextCoord = GetRoom().GetGameMap().GetRandomWalkableSquare();
                        pet.MoveTo(nextCoord.X, nextCoord.Y);
                        pet.PetData.Addexperience(10); // Give XP
                        break;
                    case 2:
                        RemovePetStatus();
                        var newX = user.X;
                        var newY = user.Y;
                        _actionTimer = 30; // Reset ActionTimer
                        if (user.RotBody == 4)
                            newY = user.Y + 1;
                        else if (user.RotBody == 0)
                            newY = user.Y - 1;
                        else if (user.RotBody == 6)
                            newX = user.X - 1;
                        else if (user.RotBody == 2)
                            newX = user.X + 1;
                        else if (user.RotBody == 3)
                        {
                            newX = user.X + 1;
                            newY = user.Y + 1;
                        }
                        else if (user.RotBody == 1)
                        {
                            newX = user.X + 1;
                            newY = user.Y - 1;
                        }
                        else if (user.RotBody == 7)
                        {
                            newX = user.X - 1;
                            newY = user.Y - 1;
                        }
                        else if (user.RotBody == 5)
                        {
                            newX = user.X - 1;
                            newY = user.Y + 1;
                        }
                        pet.PetData.Addexperience(10); // Give XP
                        pet.MoveTo(newX, newY);
                        break;
                    case 3:
                        // Remove Status
                        RemovePetStatus();
                        pet.PetData.Addexperience(10); // Give XP

                        // Add Status
                        pet.Statusses.Add("sit", TextHandling.GetString(pet.Z));
                        pet.UpdateNeeded = true;
                        _actionTimer = 25;
                        _energyTimer = 10;
                        break;
                    case 4:
                        // Remove Status
                        RemovePetStatus();

                        // Add Status
                        pet.Statusses.Add("lay", TextHandling.GetString(pet.Z));
                        pet.UpdateNeeded = true;
                        pet.PetData.Addexperience(10); // Give XP
                        _actionTimer = 30;
                        _energyTimer = 5;
                        break;
                    case 5:
                        // Remove Status
                        RemovePetStatus();

                        // Add Status
                        pet.Statusses.Add("ded", TextHandling.GetString(pet.Z));
                        pet.UpdateNeeded = true;
                        pet.PetData.Addexperience(10); // Give XP

                        // Don't move to speak for a set amount of time.
                        _speechTimer = 45;
                        _actionTimer = 30;
                        break;
                    case 6:
                        // Remove Status
                        RemovePetStatus();
                        pet.Chat("ZzzZZZzzzzZzz");
                        pet.Statusses.Add("lay", TextHandling.GetString(pet.Z));
                        pet.UpdateNeeded = true;
                        pet.PetData.Addexperience(10); // Give XP

                        // Don't move to speak for a set amount of time.
                        _energyTimer = 5;
                        _speechTimer = 30;
                        _actionTimer = 45;
                        break;
                    case 7:
                        // Remove Status
                        RemovePetStatus();

                        // Add Status
                        pet.Statusses.Add("jmp", TextHandling.GetString(pet.Z));
                        pet.UpdateNeeded = true;
                        pet.PetData.Addexperience(10); // Give XP

                        // Don't move to speak for a set amount of time.
                        _energyTimer = 5;
                        _speechTimer = 10;
                        _actionTimer = 5;
                        break;
                    case 46:
                        break;
                    default:
                        var speech = PlusEnvironment.GetGame().GetChatManager().GetPetLocale().GetValue("pet.unknowncommand");
                        pet.Chat(speech[Random.Shared.Next(0, speech.Length)]);
                        break;
                }
                pet.PetData.PetEnergy(false); // Remove Energy
            }
            else
            {
                RemovePetStatus(); // Remove Status
                if (pet.PetData.Energy < 10)
                {
                    var speech = PlusEnvironment.GetGame().GetChatManager().GetPetLocale().GetValue("pet.tired");
                    pet.Chat(speech[Random.Shared.Next(0, speech.Length)]);
                    pet.Statusses.Add("lay", TextHandling.GetString(pet.Z));
                    pet.UpdateNeeded = true;
                    _speechTimer = 50;
                    _actionTimer = 45;
                    _energyTimer = 5;
                }
                else
                {
                    var speech = PlusEnvironment.GetGame().GetChatManager().GetPetLocale().GetValue("pet.lazy");
                    pet.Chat(speech[Random.Shared.Next(0, speech.Length)]);
                    pet.PetData.PetEnergy(false); // Remove Energy
                }
            }
        }
        //Pet = null;
    }
}
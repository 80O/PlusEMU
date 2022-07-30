using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun;

internal class PetCommand : IChatCommand
{
    public string Key => "pet";
    public string PermissionRequired => "command_pet";

    public string Parameters => "";

    public string Description => "Allows you to transform into a pet..";

    public void Execute(GameClient session, Room room, string[] parameters)
    {
        var roomUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (roomUser == null)
            return;
        if (!room.PetMorphsAllowed)
        {
            session.SendWhisper("The room owner has disabled the ability to use a pet morph in this room.");
            if (session.GetHabbo().PetId > 0)
            {
                session.SendWhisper("Oops, you still have a morph, un-morphing you.");
                //Change the users Pet Id.
                session.GetHabbo().PetId = 0;

                //Quickly remove the old user instance.
                room.SendPacket(new UserRemoveComposer(roomUser.VirtualId));

                //Add the new one, they won't even notice a thing!!11 8-)
                room.SendPacket(new UsersComposer(roomUser));
            }
            return;
        }
        if (parameters.Length == 0)
        {
            session.SendWhisper("Oops, you forgot to choose the type of pet you'd like to turn into! Use :pet list to see the availiable morphs!");
            return;
        }
        if (parameters[0].ToLower() == "list")
        {
            session.SendWhisper("Habbo, Dog, Cat, Terrier, Croc, Bear, Pig, Lion, Rhino, Spider, Turtle, Chick, Frog, Drag, Monkey, Horse, Bunny, Pigeon, Demon and Gnome.");
            return;
        }
        var targetPetId = GetPetIdByString(parameters[0]);
        if (targetPetId == 0)
        {
            session.SendWhisper("Oops, couldn't find a pet by that name!");
            return;
        }

        //Change the users Pet Id.
        session.GetHabbo().PetId = targetPetId == -1 ? 0 : targetPetId;

        //Quickly remove the old user instance.
        room.SendPacket(new UserRemoveComposer(roomUser.VirtualId));

        //Add the new one, they won't even notice a thing!!11 8-)
        room.SendPacket(new UsersComposer(roomUser));

        //Tell them a quick message.
        if (session.GetHabbo().PetId > 0)
            session.SendWhisper("Use ':pet habbo' to turn back into a Habbo!");
    }

    private int GetPetIdByString(string pet)
    {
        switch (pet.ToLower())
        {
            default:
                return 0;
            case "habbo":
                return -1;
            case "dog":
                return 60; //This should be 0.
            case "cat":
                return 1;
            case "terrier":
                return 2;
            case "croc":
            case "croco":
                return 3;
            case "bear":
                return 4;
            case "liz":
            case "pig":
            case "kill":
                return 5;
            case "lion":
            case "rawr":
                return 6;
            case "rhino":
                return 7;
            case "spider":
                return 8;
            case "turtle":
                return 9;
            case "chick":
            case "chicken":
                return 10;
            case "frog":
                return 11;
            case "drag":
            case "dragon":
                return 12;
            case "monkey":
                return 14;
            case "horse":
                return 15;
            case "bunny":
                return 17;
            case "pigeon":
                return 21;
            case "demon":
                return 23;
            case "gnome":
                return 26;
        }
    }
}
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun;

internal class PushCommand : IChatCommand
{
    private readonly IGameClientManager _gameClientManager;
    public string Key => "push";
    public string PermissionRequired => "command_push";

    public string Parameters => "%target%";

    public string Description => "Push another user.";

    public PushCommand(IGameClientManager gameClientManager)
    {
        _gameClientManager = gameClientManager;
    }

    public void Execute(GameClient session, Room room, string[] @params)
    {
        if (@params.Length == 1)
        {
            session.SendWhisper("Please enter the username of the user you wish to push.");
            return;
        }
        if (!room.PushEnabled && !session.GetHabbo().GetPermissions().HasRight("room_override_custom_config"))
        {
            session.SendWhisper("Oops, it appears that the room owner has disabled the ability to use the push command in here.");
            return;
        }
        var targetClient = _gameClientManager.GetClientByUsername(@params[1]);
        if (targetClient == null)
        {
            session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
            return;
        }
        var targetUser = room.GetRoomUserManager().GetRoomUserByHabbo(targetClient.GetHabbo().Id);
        if (targetUser == null)
        {
            session.SendWhisper("An error occoured whilst finding that user, maybe they're not online or in this room.");
            return;
        }
        if (targetClient.GetHabbo().Username == session.GetHabbo().Username)
        {
            session.SendWhisper("Come on, surely you don't want to push yourself!");
            return;
        }
        if (targetUser.TeleportEnabled)
        {
            session.SendWhisper("Oops, you cannot push a user whilst they have their teleport mode enabled.");
            return;
        }
        var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (thisUser == null)
            return;
        if (!(Math.Abs(targetUser.X - thisUser.X) >= 2 || Math.Abs(targetUser.Y - thisUser.Y) >= 2))
        {
            if (targetUser.SetX - 1 == room.GetGameMap().Model.DoorX)
            {
                session.SendWhisper("Please don't push that user out of the room :(!");
                return;
            }
            if (targetUser.RotBody == 4) targetUser.MoveTo(targetUser.X, targetUser.Y + 1);
            if (thisUser.RotBody == 0) targetUser.MoveTo(targetUser.X, targetUser.Y - 1);
            if (thisUser.RotBody == 6) targetUser.MoveTo(targetUser.X - 1, targetUser.Y);
            if (thisUser.RotBody == 2) targetUser.MoveTo(targetUser.X + 1, targetUser.Y);
            if (thisUser.RotBody == 3)
            {
                targetUser.MoveTo(targetUser.X + 1, targetUser.Y);
                targetUser.MoveTo(targetUser.X, targetUser.Y + 1);
            }
            if (thisUser.RotBody == 1)
            {
                targetUser.MoveTo(targetUser.X + 1, targetUser.Y);
                targetUser.MoveTo(targetUser.X, targetUser.Y - 1);
            }
            if (thisUser.RotBody == 7)
            {
                targetUser.MoveTo(targetUser.X - 1, targetUser.Y);
                targetUser.MoveTo(targetUser.X, targetUser.Y - 1);
            }
            if (thisUser.RotBody == 5)
            {
                targetUser.MoveTo(targetUser.X - 1, targetUser.Y);
                targetUser.MoveTo(targetUser.X, targetUser.Y + 1);
            }
            room.SendPacket(new ChatComposer(thisUser.VirtualId, "*pushes " + @params[1] + "*", 0, thisUser.LastBubble));
        }
        else
            session.SendWhisper("Oops, " + @params[1] + " is not close enough!");
    }
}
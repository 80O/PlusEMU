using System;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun;

internal class PullCommand : IChatCommand
{
    private readonly IGameClientManager _gameClientManager;
    public string Key => "pull";
    public string PermissionRequired => "command_pull";

    public string Parameters => "%target%";

    public string Description => "Pull another user towards you.";

    public PullCommand(IGameClientManager gameClientManager)
    {
        _gameClientManager = gameClientManager;
    }

    public void Execute(GameClient session, Room room, string[] @params)
    {
        if (@params.Length == 1)
        {
            session.SendWhisper("Please enter the username of the user you wish to pull.");
            return;
        }
        if (!room.PullEnabled && !session.GetHabbo().GetPermissions().HasRight("room_override_custom_config"))
        {
            session.SendWhisper("Oops, it appears that the room owner has disabled the ability to use the pull command in here.");
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
            session.SendWhisper("Come on, surely you don't want to pull yourself!");
            return;
        }
        if (targetUser.TeleportEnabled)
        {
            session.SendWhisper("Oops, you cannot pull a user whilst they have their teleport mode enabled.");
            return;
        }
        var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (thisUser == null)
            return;
        if (thisUser.SetX - 1 == room.GetGameMap().Model.DoorX)
        {
            session.SendWhisper("Please don't pull that user out of the room :(!");
            return;
        }
        if (targetClient.GetHabbo().CurrentRoomId == session.GetHabbo().CurrentRoomId && Math.Abs(thisUser.X - targetUser.X) < 3 && Math.Abs(thisUser.Y - targetUser.Y) < 3)
        {
            room.SendPacket(new ChatComposer(thisUser.VirtualId, "*pulls " + @params[1] + " to them*", 0, thisUser.LastBubble));
            if (thisUser.RotBody % 2 != 0) 
                PullTarget(targetUser, thisUser.X, thisUser.Y, thisUser.direction-1);
            else
                PullTarget(targetUser, thisUser.X, thisUser.Y, thisUser.direction);
            return;
        }
        session.SendWhisper("That user is not close enough to you to be pulled, try getting closer!");
    }

    private void PullTarget(RoomUser targetUser, int X, int Y, int direction)
    {
        if (direction == 0)
            targetUser.MoveTo(X, Y-1);
        else if (direction == 2)
            targetUser.MoveTo(X+1, Y);
        else if (direction == 4)
            targetUser.MoveTo(X, Y+1);
        else if (direction == 6)
            targetUser.MoveTo(X-1, Y);
    }
}
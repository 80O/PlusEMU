using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun;

internal class PullCommand : ITargetChatCommand
{
    public string Key => "pull";
    public string PermissionRequired => "command_pull";

    public string Parameters => "%target%";

    public string Description => "Pull another user towards you.";
    public bool MustBeInSameRoom => true;

    public Task Execute(GameClient session, Room room, Habbo target, string[] parameters)
    {
        if (!room.PullEnabled && !session.GetHabbo().GetPermissions().HasRight("room_override_custom_config"))
        {
            session.SendWhisper("Oops, it appears that the room owner has disabled the ability to use the pull command in here.");
            return Task.CompletedTask;
        }
        if (target == session.GetHabbo())
        {
            session.SendWhisper("Come on, surely you don't want to pull yourself!");
            return Task.CompletedTask;
        }
        var targetUser = room.GetRoomUserManager().GetRoomUserByHabbo(target.Id);
        if (targetUser == null)
        {
            session.SendWhisper("An error occoured whilst finding that user, maybe they're not online or in this room.");
            return Task.CompletedTask;
        }
        if (targetUser.TeleportEnabled)
        {
            session.SendWhisper("Oops, you cannot pull a user whilst they have their teleport mode enabled.");
            return Task.CompletedTask;
        }
        var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (thisUser == null)
            return Task.CompletedTask;
        if (thisUser.SetX - 1 == room.GetGameMap().Model.DoorX)
        {
            session.SendWhisper("Please don't pull that user out of the room :(!");
            return Task.CompletedTask; ;
        }
        if (target.CurrentRoomId == session.GetHabbo().CurrentRoomId && Math.Abs(thisUser.X - targetUser.X) < 3 && Math.Abs(thisUser.Y - targetUser.Y) < 3)
        {
            room.SendPacket(new ChatComposer(thisUser.VirtualId, "*pulls " + parameters[1] + " to them*", 0, thisUser.LastBubble));
            if (thisUser.RotBody % 2 != 0) 
                PullTarget(targetUser, thisUser.X, thisUser.Y, thisUser.RotBody - 1);
            else
                PullTarget(targetUser, thisUser.X, thisUser.Y, thisUser.RotBody);
            return Task.CompletedTask;
        }
        session.SendWhisper("That user is not close enough to you to be pulled, try getting closer!");
        return Task.CompletedTask;
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
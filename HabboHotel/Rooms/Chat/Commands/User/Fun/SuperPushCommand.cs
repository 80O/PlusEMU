using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun;

internal class SuperPushCommand : ITargetChatCommand
{
    public string Key => "spush";
    public string PermissionRequired => "command_super_push";

    public string Parameters => "%target%";

    public string Description => "Superpush another user. (Pushes them 3 squares away)";
    public bool MustBeInSameRoom => true;

    public Task Execute(GameClient session, Room room, Habbo target, string[] parameters)
    {
        if (!room.SuperPushEnabled && !room.CheckRights(session, true) && !session.GetHabbo().GetPermissions().HasRight("room_override_custom_config"))
        {
            session.SendWhisper("Oops, it appears that the room owner has disabled the ability to use the push command in here.");
            return Task.CompletedTask;
        }
        var targetUser = room.GetRoomUserManager().GetRoomUserByHabbo(target.Id);
        if (targetUser == null)
        {
            session.SendWhisper("An error occoured whilst finding that user, maybe they're not online or in this room.");
            return Task.CompletedTask;
        }
        if (target == session.GetHabbo())
        {
            session.SendWhisper("Come on, surely you don't want to push yourself!");
            return Task.CompletedTask;
        }
        if (targetUser.TeleportEnabled)
        {
            session.SendWhisper("Oops, you cannot push a user whilst they have their teleport mode enabled.");
            return Task.CompletedTask;
        }
        var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (thisUser == null)
            return Task.CompletedTask;
        if (!(Math.Abs(targetUser.X - thisUser.X) >= 2 || Math.Abs(targetUser.Y - thisUser.Y) >= 2))
        {
            if (targetUser.SetX - 1 == room.GetGameMap().Model.DoorX || targetUser.SetY - 1 == room.GetGameMap().Model.DoorY)
            {
                session.SendWhisper("Please don't push that user out of the room :(!");
                return Task.CompletedTask;
            }
            if (targetUser.SetX - 2 == room.GetGameMap().Model.DoorX || targetUser.SetY - 2 == room.GetGameMap().Model.DoorY)
            {
                session.SendWhisper("Please don't push that user out of the room :(!");
                return Task.CompletedTask;
            }
            if (targetUser.SetX - 3 == room.GetGameMap().Model.DoorX || targetUser.SetY - 3 == room.GetGameMap().Model.DoorY)
            {
                session.SendWhisper("Please don't push that user out of the room :(!");
                return Task.CompletedTask;
            }
            if (targetUser.RotBody == 4) targetUser.MoveTo(targetUser.X, targetUser.Y + 3);
            if (thisUser.RotBody == 0) targetUser.MoveTo(targetUser.X, targetUser.Y - 3);
            if (thisUser.RotBody == 6) targetUser.MoveTo(targetUser.X - 3, targetUser.Y);
            if (thisUser.RotBody == 2) targetUser.MoveTo(targetUser.X + 3, targetUser.Y);
            if (thisUser.RotBody == 3)
            {
                targetUser.MoveTo(targetUser.X + 3, targetUser.Y);
                targetUser.MoveTo(targetUser.X, targetUser.Y + 3);
            }
            if (thisUser.RotBody == 1)
            {
                targetUser.MoveTo(targetUser.X + 3, targetUser.Y);
                targetUser.MoveTo(targetUser.X, targetUser.Y - 3);
            }
            if (thisUser.RotBody == 7)
            {
                targetUser.MoveTo(targetUser.X - 3, targetUser.Y);
                targetUser.MoveTo(targetUser.X, targetUser.Y - 3);
            }
            if (thisUser.RotBody == 5)
            {
                targetUser.MoveTo(targetUser.X - 3, targetUser.Y);
                targetUser.MoveTo(targetUser.X, targetUser.Y + 3);
            }
            room.SendPacket(new ChatComposer(thisUser.VirtualId, "*super pushes " + parameters[1] + "*", 0, thisUser.LastBubble));
        }
        else
            session.SendWhisper("Oops, " + parameters[1] + " is not close enough!");
        return Task.CompletedTask;
    }
}
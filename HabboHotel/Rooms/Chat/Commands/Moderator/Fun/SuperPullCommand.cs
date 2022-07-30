using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun;

internal class SuperPullCommand : ITargetChatCommand
{
    private readonly IGameClientManager _gameClientManager;
    public string Key => "spull";
    public string PermissionRequired => "command_super_pull";

    public string Parameters => "%username%";

    public string Description => "Pull another user to you, with no limits!";

    public bool MustBeInSameRoom => true;

    public SuperPullCommand(IGameClientManager gameClientManager)
    {
        _gameClientManager = gameClientManager;
    }

    public Task Execute(GameClient session, Room room, Habbo target, string[] parameters)
    {
        if (!room.SuperPullEnabled && !room.CheckRights(session, true) && !session.GetHabbo().GetPermissions().HasRight("room_override_custom_config"))
        {
            session.SendWhisper("Oops, it appears that the room owner has disabled the ability to use the spull command in here.");
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
            session.SendWhisper("You made the universe crash.");
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
        if (thisUser.SetX - 1 == room.GetGameMap().Model.DoorX)
        {
            session.SendWhisper("Please don't pull that user out of the room :(!");
            return Task.CompletedTask;
        }
        if (thisUser.RotBody % 2 != 0)
            thisUser.RotBody--;
        if (thisUser.RotBody == 0)
            targetUser.MoveTo(thisUser.X, thisUser.Y - 1);
        else if (thisUser.RotBody == 2)
            targetUser.MoveTo(thisUser.X + 1, thisUser.Y);
        else if (thisUser.RotBody == 4)
            targetUser.MoveTo(thisUser.X, thisUser.Y + 1);
        else if (thisUser.RotBody == 6)
            targetUser.MoveTo(thisUser.X - 1, thisUser.Y);
        room.SendPacket(new ChatComposer(thisUser.VirtualId, $"*super pulls {target.Username} to them*", 0, thisUser.LastBubble));
        return Task.CompletedTask;
    }
}
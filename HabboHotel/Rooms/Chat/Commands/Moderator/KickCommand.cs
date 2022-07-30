using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class KickCommand : ITargetChatCommand
{
    private readonly IRoomManager _roomManager;
    public string Key => "kick";
    public string PermissionRequired => "command_kick";

    public string Parameters => "%username% %reason%";

    public string Description => "Kick a user from a room and send them a reason.";

    public bool MustBeInSameRoom => false;

    public KickCommand(IRoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public Task Execute(GameClient session, Room room, Habbo target, string[] parameters)
    {
        if (target == session.GetHabbo())
        {
            session.SendWhisper("Get a life.");
            return Task.CompletedTask;
        }
        if (!target.InRoom)
        {
            session.SendWhisper("That user currently isn't in a room.");
            return Task.CompletedTask;
        }
        if (!_roomManager.TryGetRoom(target.CurrentRoomId, out var  targetRoom))
            return Task.CompletedTask;
        if (parameters.Any())
            target.GetClient().SendNotification("A moderator has kicked you from the room for the following reason: " + CommandManager.MergeParams(parameters));
        else
            target.GetClient().SendNotification("A moderator has kicked you from the room.");
        targetRoom.GetRoomUserManager().RemoveUserFromRoom(target.GetClient(), true);
        return Task.CompletedTask;
    }
}
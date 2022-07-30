using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun;

internal class FollowCommand : ITargetChatCommand
{
    private readonly IGameClientManager _gameClientManager;
    public string Key => "follow";
    public string PermissionRequired => "command_follow";

    public string Parameters => "%username%";

    public string Description => "Want to visit a specific user? Use this command!";

    public bool MustBeInSameRoom => false;

    public FollowCommand(IGameClientManager gameClientManager)
    {
        _gameClientManager = gameClientManager;
    }

    public Task Execute(GameClient session, Room room, Habbo target, string[] parameters)
    {
        if (target.CurrentRoom == session.GetHabbo().CurrentRoom)
        {
            session.SendWhisper("Hey you, open your eyes! " + target.Username + " is in this room!");
            return Task.CompletedTask;
        }
        if (target.Username == session.GetHabbo().Username)
        {
            session.SendWhisper("* Windows shutdown noise *");
            return Task.CompletedTask;
        }
        if (!target.InRoom)
        {
            session.SendWhisper("That user currently isn't in a room!");
            return Task.CompletedTask;
        }
        if (target.CurrentRoom.Access != RoomAccess.Open && !session.GetHabbo().GetPermissions().HasRight("mod_tool"))
        {
            session.SendWhisper("Oops, the room that user is either locked, passworded or invisible. You cannot follow!");
            return Task.CompletedTask;
        }
        session.GetHabbo().PrepareRoom(target.CurrentRoom.RoomId, "");
        return Task.CompletedTask;
    }
}
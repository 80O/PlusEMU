using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun;

internal class FreezeCommand : ITargetChatCommand
{
    private readonly IGameClientManager _gameClientManager;
    public string Key => "freeze";
    public string PermissionRequired => "command_freeze";

    public string Parameters => "%username%";

    public string Description => "Prevent another user from walking.";

    public bool MustBeInSameRoom => true;

    public FreezeCommand(IGameClientManager gameClientManager)
    {
        _gameClientManager = gameClientManager;
    }

    public Task Execute(GameClient session, Room room, Habbo target, string[] parameters)
    {
        var targetUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(target.Id);
        if (targetUser != null)
            targetUser.Frozen = true;
        session.SendWhisper($"Successfully froze {target.Username}!");
        return Task.CompletedTask;
    }
}
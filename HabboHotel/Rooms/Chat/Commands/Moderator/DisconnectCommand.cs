using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class DisconnectCommand : ITargetChatCommand
{
    public string Key => "dc";
    public string PermissionRequired => "command_disconnect";

    public string Parameters => "%username%";

    public string Description => "Disconnects another user from the hotel.";

    public bool MustBeInSameRoom => false;

    public Task Execute(GameClient session, Room room, Habbo target, string[] parameters)
    {
        if (target.GetPermissions().HasRight("mod_tool") && !target.GetPermissions().HasRight("mod_disconnect_any"))
        {
            session.SendWhisper("You are not allowed to Disconnect that user.");
            return Task.CompletedTask;
        }
        target.GetClient().Disconnect();
        return Task.CompletedTask;
    }
}
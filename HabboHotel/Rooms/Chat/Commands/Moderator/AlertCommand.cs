using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class AlertCommand : ITargetChatCommand
{
    public string Key => "alert";
    public string PermissionRequired => "command_alert_user";

    public string Parameters => "%username% %Messages%";

    public string Description => "Alert a user with a message of your choice.";

    public bool MustBeInSameRoom => false;

    public Task Execute(GameClient session, Room room, Habbo habbo, string[] parameters)
    {
        if (habbo.Username == session.GetHabbo().Username)
        {
            session.SendWhisper("Get a life.");
            return Task.CompletedTask;
        }
        var message = CommandManager.MergeParams(parameters);
        habbo.GetClient().SendNotification($"{session.GetHabbo().Username} alerted you with the following message:\n\n{message}");
        session.SendWhisper($"Alert successfully sent to {habbo.Username}");
        return Task.CompletedTask;
    }
}
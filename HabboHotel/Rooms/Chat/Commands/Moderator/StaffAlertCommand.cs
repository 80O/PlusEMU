using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class StaffAlertCommand : IChatCommand
{
    private readonly IGameClientManager _gameClientManager;
    public string Key => "sa";
    public string PermissionRequired => "command_staff_alert";

    public string Parameters => "%message%";

    public string Description => "Sends a message typed by you to the current online staff members.";

    public StaffAlertCommand(IGameClientManager gameClientManager)
    {
        _gameClientManager = gameClientManager;
    }

    public void Execute(GameClient session, Room room, string[] parameters)
    {
        var message = CommandManager.MergeParams(parameters);
        if (string.IsNullOrWhiteSpace(message))
        {
            session.SendWhisper("Please enter a message to send.");
            return;
        }
        _gameClientManager.StaffAlert(new BroadcastMessageAlertComposer($"Staff Alert:\r\r{message}\r\n- {session.GetHabbo().Username}"));
    }
}
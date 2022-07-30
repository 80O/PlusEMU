using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator;

internal class HalCommand : IChatCommand
{
    private readonly IGameClientManager _clientManager;
    public string Key => "hal";
    public string PermissionRequired => "command_hal";

    public string Parameters => "%message%";

    public string Description => "Send a message to the entire hotel, with a link.";

    public HalCommand(IGameClientManager clientManager)
    {
        _clientManager = clientManager;
    }

    public void Execute(GameClient session, Room room, string[] parameters)
    {
        if (parameters.Length <= 1)
        {
            session.SendWhisper("Please enter a url and a message to send..");
            return;
        }
        var url = parameters[0];
        var message = CommandManager.MergeParams(parameters, 1);
        _clientManager.SendPacket(new RoomNotificationComposer("Habboon Hotel Alert!", message + "\r\n" + "- " + session.GetHabbo().Username, "", url, url));
    }
}
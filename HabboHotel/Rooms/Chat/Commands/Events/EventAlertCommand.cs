using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Events;

internal class EventAlertCommand : IChatCommand
{
    private readonly IGameClientManager _gameClientManager;
    public string Key => "eha";
    public string PermissionRequired => "command_event_alert";

    public string Parameters => "";

    public string Description => "Send a hotel alert for your event!";

    private static DateTime? _lastEvent;

    public EventAlertCommand(IGameClientManager gameClientManager)
    {
        _gameClientManager = gameClientManager;
    }

    public void Execute(GameClient session, Room room, string[] parameters)
    {
        if (_lastEvent == null || DateTime.Now - _lastEvent > TimeSpan.FromHours(1))
        {
            _gameClientManager.SendPacket(new BroadcastMessageAlertComposer($":follow {session.GetHabbo().Username} for events! win prizes!\r\n- {session.GetHabbo().Username}"));
            _lastEvent = DateTime.Now;
        }
        else
        {
            session.SendWhisper($"Event Cooldown! {(DateTime.Now - _lastEvent).Value.Minutes} minutes left until another event can be hosted.");
        }
    }
}
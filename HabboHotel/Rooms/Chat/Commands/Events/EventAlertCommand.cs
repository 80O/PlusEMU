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

    public EventAlertCommand(IGameClientManager gameClientManager)
    {
        _gameClientManager = gameClientManager;
    }

    public void Execute(GameClient session, Room room, string[] @params)
    {
        if (session != null)
        {
            if (room != null)
            {
                if (@params.Length != 1)
                    session.SendWhisper("Invalid command! :eventalert");
                else if (!PlusEnvironment.Event)
                {
                    _gameClientManager.SendPacket(new BroadcastMessageAlertComposer(":follow " + session.GetHabbo().Username + " for events! win prizes!\r\n- " + session.GetHabbo().Username));
                    PlusEnvironment.LastEvent = DateTime.Now;
                    PlusEnvironment.Event = true;
                }
                else
                {
                    var timeSpan = DateTime.Now - PlusEnvironment.LastEvent;
                    if (timeSpan.Hours >= 1)
                    {
                        _gameClientManager.SendPacket(new BroadcastMessageAlertComposer(":follow " + session.GetHabbo().Username + " for events! win prizes!\r\n- " + session.GetHabbo().Username));
                        PlusEnvironment.LastEvent = DateTime.Now;
                    }
                    else
                    {
                        var num = checked(60 - timeSpan.Minutes);
                        session.SendWhisper("Event Cooldown! " + num + " minutes left until another event can be hosted.");
                    }
                }
            }
        }
    }
}
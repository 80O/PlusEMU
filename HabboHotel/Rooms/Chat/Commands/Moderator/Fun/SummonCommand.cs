using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun;

internal class SummonCommand : ITargetChatCommand
{
    private readonly IGameClientManager _gameClientManager;
    public string Key => "summon";
    public string PermissionRequired => "command_summon";

    public string Parameters => "%username%";

    public string Description => "Bring another user to your current room.";

    public bool MustBeInSameRoom => false;

    public SummonCommand(IGameClientManager gameClientManager)
    {
        _gameClientManager = gameClientManager;
    }

    public Task Execute(GameClient session, Room room, Habbo target, string[] parameters)
    {
        if (target.Username == session.GetHabbo().Username)
        {
            session.SendWhisper("Get a life.");
            return Task.CompletedTask;
        }
        target.GetClient().SendNotification("You have been summoned to " + session.GetHabbo().Username + "!");
        if (!target.InRoom)
            target.GetClient().Send(new RoomForwardComposer(session.GetHabbo().CurrentRoomId));
        else
            target.PrepareRoom(session.GetHabbo().CurrentRoomId, "");
        return Task.CompletedTask;
    }
}
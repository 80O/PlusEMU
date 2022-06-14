using Plus.HabboHotel.Badges;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class MassBadgeCommand : IChatCommand
{
    private readonly IGameClientManager _gameClientManager;
    private readonly IBadgeManager _badgeManager;
    public string Key => "massbadge";
    public string PermissionRequired => "command_mass_badge";

    public string Parameters => "%badge%";

    public string Description => "Give a badge to the entire hotel.";

    public MassBadgeCommand(IGameClientManager gameClientManager, IBadgeManager badgeManager)
    {
        _gameClientManager = gameClientManager;
        _badgeManager = badgeManager;
    }

    public void Execute(GameClient session, Room room, string[] @params)
    {
        if (@params.Length == 1)
        {
            session.SendWhisper("Please enter the code of the badge you'd like to give to the entire hotel.");
            return;
        }
        foreach (var client in _gameClientManager.GetClients.ToList())
        {
            if (client == null || client.GetHabbo() == null || client.GetHabbo().Username == session.GetHabbo().Username)
                continue;
            if (!client.GetHabbo().Inventory.Badges.HasBadge(@params[1]))
            {
                _badgeManager.GiveBadge(client.GetHabbo(), @params[1]).Wait();
                client.SendNotification("You have just been given a badge!");
            }
            else
                client.SendWhisper(session.GetHabbo().Username + " tried to give you a badge, but you already have it!");
        }
        session.SendWhisper("You have successfully given every user in this hotel the " + @params[1] + " badge!");
    }
}
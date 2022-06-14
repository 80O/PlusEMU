using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.Badges;

namespace Plus.Communication.Rcon.Commands.User;

internal class GiveUserBadgeCommand : IRconCommand
{
    private readonly IBadgeManager _badgeManager;
    public string Description => "This command is used to give a user a badge.";

    public string Key => "give_user_badge";
    public string Parameters => "%userId% %badgeId%";

    public GiveUserBadgeCommand(IBadgeManager badgeManager)
    {
        _badgeManager = badgeManager;
    }

    public async Task<bool> TryExecute(string[] parameters)
    {
        if (!int.TryParse(parameters[0], out var userId))
            return false;
        var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
        if (client == null || client.GetHabbo() == null)
            return false;

        // Validate the badge
        if (string.IsNullOrEmpty(Convert.ToString(parameters[1])))
            return false;
        var badge = Convert.ToString(parameters[1]);
        if (!client.GetHabbo().Inventory.Badges.HasBadge(badge))
        {
            await _badgeManager.GiveBadge(client.GetHabbo(), badge);
            client.SendPacket(new BroadcastMessageAlertComposer("You have been given a new badge!"));
        }
        return true;
    }
}
using Plus.HabboHotel.Badges;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class GiveBadgeCommand : ITargetChatCommand
{
    private readonly IBadgeManager _badgeManager;
    public string Key => "givebadge";
    public string PermissionRequired => "command_give_badge";

    public string Parameters => "%username% %badge%";

    public string Description => "Give a badge to another user.";

    public bool MustBeInSameRoom => false;

    public GiveBadgeCommand(IBadgeManager badgeManager)
    {
        _badgeManager = badgeManager;
    }

    public Task Execute(GameClient session, Room room, Habbo target, string[] parameters)
    {
        var badgeCode = parameters.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(badgeCode))
        {
            session.SendWhisper("Please enter the code of the badge you'd like to give!");
            return Task.CompletedTask;
        }
        if (!target.Inventory.Badges.HasBadge(badgeCode))
        {
            _badgeManager.GiveBadge(target, badgeCode).Wait();
            if (target.Id != session.GetHabbo().Id)
                target.GetClient().SendNotification("You have just been given a badge!");
            else
                session.SendWhisper("You have successfully given yourself the badge " + parameters[2] + "!");
        }
        else
            session.SendWhisper("Oops, that user already has this badge (" + parameters[2] + ") !");
        return Task.CompletedTask;
    }
}
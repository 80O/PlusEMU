using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator;

internal class BubbleCommand : IChatCommand
{
    private readonly IChatStyleManager _chatStyleManager;
    public string Key => "bubble";
    public string PermissionRequired => "command_bubble";

    public string Parameters => "%id%";

    public string Description => "Use a custom bubble to chat with.";

    public BubbleCommand(IChatStyleManager chatStyleManager)
    {
        _chatStyleManager = chatStyleManager;
    }

    public void Execute(GameClient session, Room room, string[] parameters)
    {
        var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (user == null)
            return;
        if (parameters.Length == 0)
        {
            session.SendWhisper("Oops, you forgot to enter a bubble ID!");
            return;
        }
        if (!int.TryParse(parameters[0], out var bubble))
        {
            session.SendWhisper("Please enter a valid number.");
            return;
        }
        if (!_chatStyleManager.TryGetStyle(bubble, out var style) || style.RequiredRight.Length > 0 && !session.GetHabbo().GetPermissions().HasRight(style.RequiredRight))
        {
            session.SendWhisper("Oops, you cannot use this bubble due to a rank requirement, sorry!");
            return;
        }
        user.LastBubble = bubble;
        session.GetHabbo().CustomBubbleId = bubble;
        session.SendWhisper("Bubble set to: " + bubble);
    }
}
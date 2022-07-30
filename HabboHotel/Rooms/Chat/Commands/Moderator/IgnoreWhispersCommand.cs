using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class IgnoreWhispersCommand : IChatCommand
{
    public string Key => "ignorewhispers";
    public string PermissionRequired => "command_ignore_whispers";

    public string Parameters => "";

    public string Description => "Allows you to ignore all of the whispers in the room, except from your own.";

    public void Execute(GameClient session, Room room, string[] parameters)
    {
        session.GetHabbo().IgnorePublicWhispers = !session.GetHabbo().IgnorePublicWhispers;
        session.SendWhisper($"You're {(session.GetHabbo().IgnorePublicWhispers ? "now" : "no longer")} ignoring public whispers!");
    }
}
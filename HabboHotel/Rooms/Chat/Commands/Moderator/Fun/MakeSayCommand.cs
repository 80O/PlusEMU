using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun;

internal class MakeSayCommand : ITargetChatCommand
{
    public string Key => "makesay";
    public string PermissionRequired => "command_makesay";

    public string Parameters => "%username% %message%";

    public string Description => "Forces the specified user to say the specified message.";

    public bool MustBeInSameRoom => true;

    public Task Execute(GameClient session, Room room, Habbo target, string[] parameters)
    {
        if (!parameters.Any())
            session.SendWhisper("You must enter a username and the message you wish to force them to say.");
        else
        {
            var message = CommandManager.MergeParams(parameters);
            var targetUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(target.Id);
            if (targetUser != null)
            {
                if (targetUser.GetClient() != null && targetUser.GetClient().GetHabbo() != null)
                {
                    if (!targetUser.GetClient().GetHabbo().GetPermissions().HasRight("mod_make_say_any"))
                        room.SendPacket(new ChatComposer(targetUser.VirtualId, message, 0, targetUser.LastBubble));
                    else
                        session.SendWhisper("You cannot use makesay on this user.");
                }
            }
            else
                session.SendWhisper("This user could not be found in the room");
        }
        return Task.CompletedTask;
    }
}
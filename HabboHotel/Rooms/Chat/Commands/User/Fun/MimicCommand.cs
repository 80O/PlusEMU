using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun;

internal class MimicCommand : ITargetChatCommand
{
    private readonly IDatabase _database;
    public string Key => "mimic";
    public string PermissionRequired => "command_mimic";

    public string Parameters => "%username%";

    public string Description => "Liking someone elses swag? Copy it!";
    public bool MustBeInSameRoom => true;

    public MimicCommand(IDatabase database)
    {
        _database = database;
    }

    public Task Execute(GameClient session, Room room, Habbo target, string[] parameters)
    {
        if (!target.AllowMimic)
        {
            session.SendWhisper("Oops, you cannot mimic this user - sorry!");
            return Task.CompletedTask;
        }
        var targetUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(target.Id);
        if (targetUser == null)
        {
            session.SendWhisper("An error occoured whilst finding that user, maybe they're not online or in this room.");
            return Task.CompletedTask;
        }
        session.GetHabbo().Gender = targetUser.GetClient().GetHabbo().Gender;
        session.GetHabbo().Look = targetUser.GetClient().GetHabbo().Look;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("UPDATE `users` SET `gender` = @gender, `look` = @look WHERE `id` = @id LIMIT 1");
            dbClient.AddParameter("gender", session.GetHabbo().Gender);
            dbClient.AddParameter("look", session.GetHabbo().Look);
            dbClient.AddParameter("id", session.GetHabbo().Id);
            dbClient.RunQuery();
        }
        var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (user != null)
        {
            session.Send(new AvatarAspectUpdateComposer(session.GetHabbo().Look, session.GetHabbo().Gender));
            session.Send(new UserChangeComposer(user, true));
            room.SendPacket(new UserChangeComposer(user, false));
        }
        return Task.CompletedTask;
    }
}
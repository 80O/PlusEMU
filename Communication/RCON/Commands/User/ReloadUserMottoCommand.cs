using Plus.Communication.Packets.Outgoing.Rooms.Engine;

namespace Plus.Communication.Rcon.Commands.User;

internal class ReloadUserMottoCommand : IRconCommand
{
    public string Description => "This command is used to reload the users motto from the database.";

    public string Key => "reload_user_motto";
    public string Parameters => "%userId%";

    public Task<bool> TryExecute(string[] parameters)
    {
        if (!int.TryParse(parameters[0], out var userId))
            return Task.FromResult(false);
        var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
        if (client == null || client.GetHabbo() == null)
            return Task.FromResult(false);
        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `motto` FROM `users` WHERE `id` = @userID LIMIT 1");
            dbClient.AddParameter("userID", userId);
            client.GetHabbo().Motto = dbClient.GetString();
        }

        // If we're in a room, we cannot really send the packets, so flag this as completed successfully, since we already updated it.
        if (!client.GetHabbo().InRoom)
            return Task.FromResult(true);
        //We are in a room, let's try to run the packets.
        var room = client.GetHabbo().CurrentRoom;
        if (room != null)
        {
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(client.GetHabbo().Id);
            if (user != null)
            {
                room.SendPacket(new UserChangeComposer(user, false));
                return Task.FromResult(true);
            }
        }
        return Task.FromResult(false);
    }
}
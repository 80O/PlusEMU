using Plus.Communication.Packets.Outgoing.Inventory.Bots;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Inventory.Bots;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User;

internal class KickBotsCommand : IChatCommand
{
    private readonly IDatabase _database;
    public string Key => "kickbots";
    public string PermissionRequired => "command_kickbots";

    public string Parameters => "";

    public string Description => "Kick all of the bots from the room.";

    public KickBotsCommand(IDatabase database)
    {
        _database = database;
    }

    public void Execute(GameClient session, Room room, string[] @params)
    {
        if (!room.CheckRights(session, true))
        {
            session.SendWhisper("Oops, only the room owner can run this command!");
            return;
        }
        foreach (var user in room.GetRoomUserManager().GetUserList().ToList())
        {
            if (user == null || user.IsPet || !user.IsBot)
                continue;
            RoomUser botUser = null;
            if (!room.GetRoomUserManager().TryGetBot(user.BotData.Id, out botUser))
                return;
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `bots` SET `room_id` = '0' WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("id", user.BotData.Id);
                dbClient.RunQuery();
            }
            session.GetHabbo().Inventory.Bots.AddBot(new Bot(Convert.ToInt32(botUser.BotData.Id), Convert.ToInt32(botUser.BotData.OwnerId), botUser.BotData.Name, botUser.BotData.Motto,
                botUser.BotData.Look, botUser.BotData.Gender));
            session.SendPacket(new BotInventoryComposer(session.GetHabbo().Inventory.Bots.Bots.Values.ToList()));
            room.GetRoomUserManager().RemoveBot(botUser.VirtualId, false);
        }
        session.SendWhisper("Success, removed all bots.");
    }
}
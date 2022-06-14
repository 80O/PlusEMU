using Plus.Database;
using Plus.HabboHotel.GameClients;
using System.Data;
using System.Text;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class UserInfoCommand : IChatCommand
{
    private readonly IDatabase _database;
    private readonly IGameClientManager _gameClientManager;
    public string Key => "userinfo";
    public string PermissionRequired => "command_user_info";

    public string Parameters => "%username%";

    public string Description => "View another users profile information.";

    public UserInfoCommand(IDatabase database, IGameClientManager gameClientManager)
    {
        _database = database;
        _gameClientManager = gameClientManager;
    }

    public void Execute(GameClient session, Room room, string[] @params)
    {
        if (@params.Length == 1)
        {
            session.SendWhisper("Please enter the username of the user you wish to view.");
            return;
        }
        DataRow userData = null;
        DataRow userInfo = null;
        var username = @params[1];
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery(
                "SELECT `id`,`username`,`mail`,`rank`,`motto`,`credits`,`activity_points`,`vip_points`,`gotw_points`,`online`,`rank_vip` FROM users WHERE `username` = @Username LIMIT 1");
            dbClient.AddParameter("Username", username);
            userData = dbClient.GetRow();
        }
        if (userData == null)
        {
            session.SendNotification("Oops, there is no user in the database with that username (" + username + ")!");
            return;
        }
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + Convert.ToInt32(userData["id"]) + "' LIMIT 1");
            userInfo = dbClient.GetRow();
            if (userInfo == null)
            {
                dbClient.RunQuery("INSERT INTO `user_info` (`user_id`) VALUES ('" + Convert.ToInt32(userData["id"]) + "')");
                dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + Convert.ToInt32(userData["id"]) + "' LIMIT 1");
                userInfo = dbClient.GetRow();
            }
        }
        var targetClient = _gameClientManager.GetClientByUsername(username);
        var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Convert.ToDouble(userInfo["trading_locked"]));
        var habboInfo = new StringBuilder();
        habboInfo.Append(Convert.ToString(userData["username"]) + "'s account:\r\r");
        habboInfo.Append("Generic Info:\r");
        habboInfo.Append("ID: " + Convert.ToInt32(userData["id"]) + "\r");
        habboInfo.Append("Rank: " + Convert.ToInt32(userData["rank"]) + "\r");
        habboInfo.Append("VIP Rank: " + Convert.ToInt32(userData["rank_vip"]) + "\r");
        habboInfo.Append("Email: " + Convert.ToString(userData["mail"]) + "\r");
        habboInfo.Append("Online Status: " + (targetClient != null ? "True" : "False") + "\r\r");
        habboInfo.Append("Currency Info:\r");
        habboInfo.Append("Credits: " + Convert.ToInt32(userData["credits"]) + "\r");
        habboInfo.Append("Duckets: " + Convert.ToInt32(userData["activity_points"]) + "\r");
        habboInfo.Append("Diamonds: " + Convert.ToInt32(userData["vip_points"]) + "\r");
        habboInfo.Append("GOTW Points: " + Convert.ToInt32(userData["gotw_points"]) + "\r\r");
        habboInfo.Append("Moderation Info:\r");
        habboInfo.Append("Bans: " + Convert.ToInt32(userInfo["bans"]) + "\r");
        habboInfo.Append("CFHs Sent: " + Convert.ToInt32(userInfo["cfhs"]) + "\r");
        habboInfo.Append("Abusive CFHs: " + Convert.ToInt32(userInfo["cfhs_abusive"]) + "\r");
        habboInfo.Append("Trading Locked: " + (Convert.ToInt32(userInfo["trading_locked"]) == 0 ? "No outstanding lock" : "Expiry: " + origin.ToString("dd/MM/yyyy") + "") + "\r");
        habboInfo.Append("Amount of trading locks: " + Convert.ToInt32(userInfo["trading_locks_count"]) + "\r\r");
        if (targetClient != null)
        {
            habboInfo.Append("Current Session:\r");
            if (!targetClient.GetHabbo().InRoom)
                habboInfo.Append("Currently not in a room.\r");
            else
            {
                habboInfo.Append("Room: " + targetClient.GetHabbo().CurrentRoom.Name + " (" + targetClient.GetHabbo().CurrentRoom.RoomId + ")\r");
                habboInfo.Append("Room Owner: " + targetClient.GetHabbo().CurrentRoom.OwnerName + "\r");
                habboInfo.Append("Current Visitors: " + targetClient.GetHabbo().CurrentRoom.UserCount + "/" + targetClient.GetHabbo().CurrentRoom.UsersMax);
            }
        }
        session.SendNotification(habboInfo.ToString());
    }
}
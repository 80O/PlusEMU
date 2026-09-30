using System.Data;
using Dapper;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User;

internal class ConvertCreditsCommand : IChatCommand
{
    private readonly IDatabase _database;
    public string Key => "convertcredits";
    public string PermissionRequired => "command_convert_credits";

    public string Parameters => "";

    public string Description => "Convert your exchangeable furniture into actual credits.";

    public ConvertCreditsCommand(IDatabase database)
    {
        _database = database;
    }

    public void Execute(GameClient session, Room room, string[] parameters)
    {
        lock (session.GetHabbo().Inventory.Furniture.SyncRoot)
        {
            var totalValue = 0;
            try
            {
                DataTable table = null;
                using (var dbClient = _database.GetQueryReactor())
                {
                    dbClient.SetQuery($"SELECT `id` FROM `items` WHERE `user_id` = '{session.GetHabbo().Id}' AND (`room_id`=  '0' OR `room_id` = '')");
                    table = dbClient.GetTable();
                }
                if (table == null)
                {
                    session.SendWhisper("You currently have no items in your inventory!");
                    return;
                }
                if (table.Rows.Count > 0)
                {
                    using var connection = _database.Connection();
                    connection.Open();
                    foreach (DataRow row in table.Rows)
                    {
                        var item = session.GetHabbo().Inventory.Furniture.GetItem(Convert.ToUInt32(row[0]));
                        if (item == null || item.Definition.InteractionType != InteractionType.Exchange)
                            continue;
                        var value = item.Definition.BehaviourData;
                        if (connection.Execute("DELETE FROM items WHERE id = @Id AND user_id = @UserId AND room_id = 0",
                                new { item.Id, UserId = session.GetHabbo().Id }) != 1)
                            continue;
                        session.GetHabbo().Inventory.Furniture.RemoveItem(item.Id);
                        session.Send(new FurniListRemoveComposer(item.Id));
                        totalValue += value;
                        if (value > 0)
                        {
                            session.GetHabbo().AdjustCredits(value);
                            session.Send(new CreditBalanceComposer(session.GetHabbo().Credits));
                        }
                    }
                }
                if (totalValue > 0)
                    session.SendNotification($"All credits have successfully been converted!\r\r(Total value: {totalValue} credits!");
                else
                    session.SendNotification("It appears you don't have any exchangeable items!");
            }
            catch
            {
                session.SendNotification("Oops, an error occoured whilst converting your credits!");
            }
        }
    }
}

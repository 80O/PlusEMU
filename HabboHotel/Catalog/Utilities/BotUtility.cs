using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Users.Inventory.Bots;
using System.Data;

namespace Plus.HabboHotel.Catalog.Utilities;

public static class BotUtility
{
    public static Bot CreateBot(ItemData itemData, int ownerId)
    {
        DataRow bot = null;
        if (!PlusEnvironment.GetGame().GetCatalog().TryGetBot(itemData.Id, out var cataBot))
            return null;
        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            dbClient.SetQuery("INSERT INTO bots (`user_id`,`name`,`motto`,`look`,`gender`,`ai_type`) VALUES ('" + ownerId + "', '" + cataBot.Name + "', '" + cataBot.Motto + "', '" + cataBot.Figure +
                              "', '" + cataBot.Gender + "', '" + cataBot.AiType + "')");
            var id = Convert.ToInt32(dbClient.InsertQuery());
            dbClient.SetQuery("SELECT `id`,`user_id`,`name`,`motto`,`look`,`gender` FROM `bots` WHERE `user_id` = '" + ownerId + "' AND `id` = '" + id + "' LIMIT 1");
            bot = dbClient.GetRow();
        }
        return new Bot(Convert.ToInt32(bot["id"]), Convert.ToInt32(bot["user_id"]), Convert.ToString(bot["name"]), Convert.ToString(bot["motto"]), Convert.ToString(bot["look"]),
            Convert.ToString(bot["gender"]));
    }


    public static BotAiType GetAiFromString(string type)
    {
        switch (type)
        {
            case "pet":
                return BotAiType.Pet;
            case "generic":
                return BotAiType.Generic;
            case "bartender":
                return BotAiType.Bartender;
            case "casino_bot":
                return BotAiType.CasinoBot;
            default:
                return BotAiType.Generic;
        }
    }
}
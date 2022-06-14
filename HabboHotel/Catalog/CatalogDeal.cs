using Plus.HabboHotel.Items;
using System.Data;

namespace Plus.HabboHotel.Catalog;

public class CatalogDeal
{
    public CatalogDeal(int id, string items, string displayName, int roomId, IItemDataManager itemDataManager)
    {
        Id = id;
        DisplayName = displayName;
        RoomId = roomId;
        ItemDataList = new List<CatalogItem>();
        if (roomId != 0)
        {
            DataTable data = null;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `items`.base_item, COALESCE(`items_groups`.`group_id`, 0) AS `group_id` FROM `items` LEFT OUTER JOIN `items_groups` ON `items`.`id` = `items_groups`.`id` WHERE `items`.`room_id` = @rid;");
                dbClient.AddParameter("rid", roomId);
                data = dbClient.GetTable();
            }
            var roomItems = new Dictionary<int, int>();
            if (data != null)
            {
                foreach (DataRow dRow in data.Rows)
                {
                    var itemId = Convert.ToInt32(dRow["base_item"]);
                    if (roomItems.ContainsKey(itemId))
                        roomItems[itemId]++;
                    else
                        roomItems.Add(itemId, 1);
                }
            }
            foreach (var roomItem in roomItems) items += roomItem.Key + "*" + roomItem.Value + ";";
            if (roomItems.Count > 0) items = items.Remove(items.Length - 1);
        }
        var splitItems = items.Split(';');
        foreach (var split in splitItems)
        {
            var item = split.Split('*');
            if (!int.TryParse(item[0], out var itemId) || !int.TryParse(item[1], out var amount))
                continue;
            if (!itemDataManager.GetItem(itemId, out var data))
                continue;
            ItemDataList.Add(new CatalogItem(0, itemId, data, string.Empty, 0, 0, 0, 0, amount, 0, 0, false, "", "", 0));
        }
    }

    public int Id { get; set; }
    public List<CatalogItem> ItemDataList { get; }
    public string DisplayName { get; set; }
    public int RoomId { get; set; }
}
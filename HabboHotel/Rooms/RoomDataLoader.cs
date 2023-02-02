using System.Data;

namespace Plus.HabboHotel.Rooms;

public static class RoomDataLoader
{
    public static List<RoomData> GetRoomsDataByOwnerSortByName(int ownerId)
    {
        var data = new List<RoomData>();
        using var dbClient = PlusEnvironment.DatabaseManager.GetQueryReactor();
        dbClient.SetQuery("SELECT `username`, `rooms`.* FROM `users` INNER JOIN `rooms` ON `owner` = `users`.`id` WHERE `users`.`id` = @ownerid ORDER BY `caption`;");
        dbClient.AddParameter("ownerid", ownerId);
        var rooms = dbClient.GetTable();
        if (rooms != null)
        {
            foreach (DataRow row in rooms.Rows)
            {
                if (PlusEnvironment.Game.RoomManager.TryGetRoom(Convert.ToUInt32(row["id"]), out var room))
                    data.Add(room);
                else
                {
                    if (!PlusEnvironment.Game.RoomManager.TryGetModel(Convert.ToString(row["model_name"]), out var model)) continue;
                    data.Add(new(Convert.ToUInt32(row["id"]), Convert.ToString(row["caption"]), Convert.ToString(row["model_name"]), Convert.ToString(row["username"]),
                        Convert.ToInt32(row["owner"]),
                        Convert.ToString(row["password"]), Convert.ToInt32(row["score"]), Convert.ToString(row["roomtype"]), Convert.ToString(row["state"]), Convert.ToInt32(row["users_now"]),
                        Convert.ToInt32(row["users_max"]), Convert.ToInt32(row["category"]), Convert.ToString(row["description"]), Convert.ToString(row["tags"]), Convert.ToString(row["floor"]),
                        Convert.ToString(row["landscape"]), Convert.ToString(row["allow_pets"]) == "1", Convert.ToString(row["allow_pets_eat"]) == "1", Convert.ToString(row["room_blocking_disabled"]) == "1",
                        Convert.ToString(row["allow_hidewall"]) == "1",
                        Convert.ToInt32(row["wallthick"]), Convert.ToInt32(row["floorthick"]), Convert.ToString(row["wallpaper"]), Convert.ToInt32(row["mute_settings"]),
                        Convert.ToInt32(row["ban_settings"]),
                        Convert.ToInt32(row["kick_settings"]), Convert.ToInt32(row["chat_mode"]), Convert.ToInt32(row["chat_size"]), Convert.ToInt32(row["chat_speed"]),
                        Convert.ToInt32(row["chat_extra_flood"]),
                        Convert.ToInt32(row["chat_hearing_distance"]), Convert.ToInt32(row["trade_settings"]), Convert.ToString(row["push_enabled"]) == "1",
                        Convert.ToString(row["pull_enabled"]) == "1",
                        Convert.ToString(row["spush_enabled"]) == "1", Convert.ToString(row["spull_enabled"]) == "1", Convert.ToString(row["enables_enabled"]) == "1",
                        Convert.ToString(row["respect_notifications_enabled"]) == "1",
                        Convert.ToString(row["pet_morphs_allowed"]) == "1", Convert.ToInt32(row["group_id"]), Convert.ToInt32(row["sale_price"]), Convert.ToString(row["lay_enabled"]) == "1", model));
                }
            }
        }
        return data;
    }

    public static bool TryGetData(uint roomId, out RoomData data)
    {
        if (PlusEnvironment.Game.RoomManager.TryGetRoom(roomId, out var room))
        {
            data = room;
            return true;
        }
        using (var dbClient = PlusEnvironment.DatabaseManager.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `rooms`.*, `users`.`username` FROM `rooms` INNER JOIN `users` ON `users`.`id` = `rooms`.`owner` WHERE `rooms`.`id` = @id LIMIT 1");
            dbClient.AddParameter("id", roomId);
            var row = dbClient.GetRow();
            if (row != null)
            {
                RoomModel model = null;
                if (!PlusEnvironment.Game.RoomManager.TryGetModel(Convert.ToString(row["model_name"]), out model))
                {
                    data = null;
                    return false;
                }
                
                var username = !string.IsNullOrEmpty(Convert.ToString(row["username"])) ? Convert.ToString(row["username"]) : "Habboon";
                data = new(Convert.ToUInt32(row["id"]), Convert.ToString(row["caption"]), Convert.ToString(row["model_name"]), username, Convert.ToInt32(row["owner"]),
                    Convert.ToString(row["password"]), Convert.ToInt32(row["score"]), Convert.ToString(row["roomtype"]), Convert.ToString(row["state"]), Convert.ToInt32(row["users_now"]),
                    Convert.ToInt32(row["users_max"]), Convert.ToInt32(row["category"]), Convert.ToString(row["description"]), Convert.ToString(row["tags"]), Convert.ToString(row["floor"]),
                    Convert.ToString(row["landscape"]), Convert.ToString(row["allow_pets"]) == "1", Convert.ToString(row["allow_pets_eat"]) == "1", Convert.ToString(row["room_blocking_disabled"]) == "1",
                    Convert.ToString(row["allow_hidewall"]) == "1",
                    Convert.ToInt32(row["wallthick"]), Convert.ToInt32(row["floorthick"]), Convert.ToString(row["wallpaper"]), Convert.ToInt32(row["mute_settings"]),
                    Convert.ToInt32(row["ban_settings"]),
                    Convert.ToInt32(row["kick_settings"]), Convert.ToInt32(row["chat_mode"]), Convert.ToInt32(row["chat_size"]), Convert.ToInt32(row["chat_speed"]),
                    Convert.ToInt32(row["chat_extra_flood"]),
                    Convert.ToInt32(row["chat_hearing_distance"]), Convert.ToInt32(row["trade_settings"]), Convert.ToString(row["push_enabled"]) == "1", Convert.ToString(row["pull_enabled"]) == "1",
                    Convert.ToString(row["spush_enabled"]) == "1", Convert.ToString(row["spull_enabled"]) == "1", Convert.ToString(row["enables_enabled"]) == "1",
                    Convert.ToString(row["respect_notifications_enabled"]) == "1",
                    Convert.ToString(row["pet_morphs_allowed"]) == "1", Convert.ToInt32(row["group_id"]), Convert.ToInt32(row["sale_price"]), Convert.ToString(row["lay_enabled"]) == "1", model);
                return true;
            }
        }
        data = null;
        return false;
    }
}
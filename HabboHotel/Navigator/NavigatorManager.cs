using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dapper;
using NLog;
using Plus.Database;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Navigator.SavedSearches;

namespace Plus.HabboHotel.Navigator;

public sealed class NavigatorManager : INavigatorManager
{
    private readonly IDatabase _database;
    private static readonly ILogger Log = LogManager.GetLogger("Plus.HabboHotel.Navigator.NavigatorManager");

    private readonly Dictionary<int, FeaturedRoom> _featuredRooms;
    private readonly Dictionary<int, SearchResultList> _searchResultLists;
    private readonly Dictionary<int, TopLevelItem> _topLevelItems;

    public NavigatorManager(IDatabase database)
    {
        _database = database;
        _topLevelItems = new Dictionary<int, TopLevelItem>();
        _searchResultLists = new Dictionary<int, SearchResultList>();

        //Does this need to be dynamic?
        _topLevelItems.Add(1, new TopLevelItem(1, "official_view", "", ""));
        _topLevelItems.Add(2, new TopLevelItem(2, "hotel_view", "", ""));
        _topLevelItems.Add(3, new TopLevelItem(3, "roomads_view", "", ""));
        _topLevelItems.Add(4, new TopLevelItem(4, "myworld_view", "", ""));
        _featuredRooms = new Dictionary<int, FeaturedRoom>();
    }

    public void Init()
    {
        if (_searchResultLists.Count > 0)
            _searchResultLists.Clear();
        if (_featuredRooms.Count > 0)
            _featuredRooms.Clear();
        DataTable table = null;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT * FROM `navigator_categories` ORDER BY `id` ASC");
            table = dbClient.GetTable();
            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    if (Convert.ToInt32(row["enabled"]) == 1)
                    {
                        if (!_searchResultLists.ContainsKey(Convert.ToInt32(row["id"])))
                        {
                            _searchResultLists.Add(Convert.ToInt32(row["id"]),
                                new SearchResultList(Convert.ToInt32(row["id"]), Convert.ToString(row["category"]), Convert.ToString(row["category_identifier"]), Convert.ToString(row["public_name"]),
                                    true, -1, Convert.ToInt32(row["required_rank"]), NavigatorViewModeUtility.GetViewModeByString(Convert.ToString(row["view_mode"])),
                                    Convert.ToString(row["category_type"]), Convert.ToString(row["search_allowance"]), Convert.ToInt32(row["order_id"])));
                        }
                    }
                }
            }
            dbClient.SetQuery("SELECT `room_id`,`caption`,`description`,`image_url`,`enabled` FROM `navigator_publics` ORDER BY `order_num` ASC");
            var getPublics = dbClient.GetTable();
            if (getPublics != null)
            {
                foreach (DataRow row in getPublics.Rows)
                {
                    if (Convert.ToInt32(row["enabled"]) == 1)
                    {
                        if (!_featuredRooms.ContainsKey(Convert.ToInt32(row["room_id"])))
                        {
                            _featuredRooms.Add(Convert.ToInt32(row["room_id"]),
                                new FeaturedRoom(Convert.ToInt32(row["room_id"]), Convert.ToString(row["caption"]), Convert.ToString(row["description"]), Convert.ToString(row["image_url"])));
                        }
                    }
                }
            }
        }
        Log.Info("Navigator -> LOADED");
    }

    public List<SearchResultList> GetCategorysForSearch(string category)
    {
        var categorys =
            from cat in _searchResultLists
            where cat.Value.Category == category
            orderby cat.Value.OrderId
            select cat.Value;
        return categorys.ToList();
    }

    public ICollection<SearchResultList> GetResultByIdentifier(string category)
    {
        var categorys =
            from cat in _searchResultLists
            where cat.Value.CategoryIdentifier == category
            orderby cat.Value.OrderId
            select cat.Value;
        return categorys.ToList();
    }

    public ICollection<SearchResultList> GetFlatCategories()
    {
        var categorys =
            from cat in _searchResultLists
            where cat.Value.CategoryType == NavigatorCategoryType.Category
            orderby cat.Value.OrderId
            select cat.Value;
        return categorys.ToList();
    }

    public ICollection<SearchResultList> GetEventCategories()
    {
        var categorys =
            from cat in _searchResultLists
            where cat.Value.CategoryType == NavigatorCategoryType.PromotionCategory
            orderby cat.Value.OrderId
            select cat.Value;
        return categorys.ToList();
    }

    public ICollection<TopLevelItem> GetTopLevelItems() => _topLevelItems.Values;

    public ICollection<SearchResultList> GetSearchResultLists() => _searchResultLists.Values;

    public bool TryGetTopLevelItem(int id, out TopLevelItem topLevelItem) => _topLevelItems.TryGetValue(id, out topLevelItem);

    public bool TryGetSearchResultList(int id, out SearchResultList searchResultList) => _searchResultLists.TryGetValue(id, out searchResultList);

    public bool TryGetFeaturedRoom(int roomId, out FeaturedRoom publicRoom) => _featuredRooms.TryGetValue(roomId, out publicRoom);

    public ICollection<FeaturedRoom> GetFeaturedRooms() => _featuredRooms.Values;

    public async Task<Dictionary<int, SavedSearch>> LoadUserNavigatorPreferences(int userId)
    {
        using var connection = _database.Connection();
        return (await connection.QueryAsync<SavedSearch>("SELECT `id`,`filter`,`search_code` as search FROM `user_saved_searches` WHERE `user_id` = @userId",
            new
            {
                userId
            })).ToDictionary(search => search.Id);
    }

    public async Task SaveHomeRoom(Habbo habbo, int roomId)
    {
        habbo.HomeRoom = roomId;

        if (!RoomFactory.TryGetData(roomId, out var _))
            return;

        using var connection = _database.Connection();
        await connection.ExecuteAsync("UPDATE users SET home_room = @roomid WHERE id = @userid LIMIT 1",
            new { roomid = roomId, userid = habbo.Id });
    }
}
using System.Collections.Concurrent;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Rooms.Games;

public class GameItemHandler
{
    private ConcurrentDictionary<int, Item> _banzaiPyramids;
    private ConcurrentDictionary<int, Item> _banzaiTeleports;
    private Room _room;

    public GameItemHandler(Room room)
    {
        _room = room;
        _banzaiPyramids = new ConcurrentDictionary<int, Item>();
        _banzaiTeleports = new ConcurrentDictionary<int, Item>();
    }

    public void OnCycle()
    {
        CyclePyramids();
    }

    private void CyclePyramids()
    {
        foreach (var item in _banzaiPyramids.Values.ToList())
        {
            if (item == null)
                continue;
            if (item.InteractionCountHelper == 0 && item.ExtraData == "1")
            {
                _room.GetGameMap().RemoveFromMap(item, false);
                item.InteractionCountHelper = 1;
            }
            if (string.IsNullOrEmpty(item.ExtraData))
                item.ExtraData = "0";
            var randomNumber = Random.Shared.Next(0, 30);
            if (randomNumber == 15)
            {
                if (item.ExtraData == "0")
                {
                    item.ExtraData = "1";
                    item.UpdateState();
                    _room.GetGameMap().RemoveFromMap(item, false);
                }
                else
                {
                    if (_room.GetGameMap().ItemCanBePlaced(item.GetX, item.GetY))
                    {
                        item.ExtraData = "0";
                        item.UpdateState();
                        _room.GetGameMap().AddItemToMap(item);
                    }
                }
            }
        }
    }

    public void AddPyramid(Item item, int itemId)
    {
        if (_banzaiPyramids.ContainsKey(itemId))
            _banzaiPyramids[itemId] = item;
        else
            _banzaiPyramids.TryAdd(itemId, item);
    }

    public void RemovePyramid(int itemId)
    {
        _banzaiPyramids.TryRemove(itemId, out var item);
    }

    public void AddTeleport(Item item, int itemId)
    {
        if (_banzaiTeleports.ContainsKey(itemId))
            _banzaiTeleports[itemId] = item;
        else
            _banzaiTeleports.TryAdd(itemId, item);
    }

    public void RemoveTeleport(int itemId)
    {
        _banzaiTeleports.TryRemove(itemId, out var item);
    }

    public void OnTeleportRoomUserEnter(RoomUser user, Item item)
    {
        if (_banzaiTeleports.Count <= 1) 
            return;

        var banzais = _banzaiTeleports.Values.Where(_banzai => _banzai.Id != item.Id).ToArray();
        var banzai = banzais[Random.Shared.Next(0, banzais.Length)];

        banzai.ExtraData = "1";
        banzai.UpdateNeeded = true;
        banzai.UpdateState();

        _room.GetGameMap().TeleportToItem(user, banzai);

    }

    public void Dispose()
    {
        if (_banzaiTeleports != null)
            _banzaiTeleports.Clear();
        if (_banzaiPyramids != null)
            _banzaiPyramids.Clear();
        _banzaiPyramids = null;
        _banzaiTeleports = null;
        _room = null;
    }
}
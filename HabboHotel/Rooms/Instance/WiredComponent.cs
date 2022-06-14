using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Items.Wired.Boxes;
using Plus.HabboHotel.Items.Wired.Boxes.Conditions;
using Plus.HabboHotel.Items.Wired.Boxes.Effects;
using Plus.HabboHotel.Items.Wired.Boxes.Triggers;
using System.Collections.Concurrent;
using System.Data;

namespace Plus.HabboHotel.Rooms.Instance;

public class WiredComponent
{
    private readonly Room _room;
    private readonly ConcurrentDictionary<int, IWiredItem> _wiredItems;

    public WiredComponent(Room instance) //, RoomItem Items)
    {
        _room = instance;
        _wiredItems = new ConcurrentDictionary<int, IWiredItem>();
    }

    public void OnCycle()
    {
        var start = DateTime.Now;
        foreach (var item in _wiredItems.ToList())
        {
            var selectedItem = _room.GetRoomItemHandler().GetItem(item.Value.Item.Id);
            if (selectedItem == null)
                TryRemove(item.Key);
            if (item.Value is IWiredCycle)
            {
                var cycle = (IWiredCycle)item.Value;
                if (cycle.TickCount <= 0)
                    cycle.OnCycle();
                else
                    cycle.TickCount--;
            }
        }
        var span = DateTime.Now - start;
        if (span.Milliseconds > 400)
        {
            //log.Warn("<Room " + _room.Id + "> Wired took " + Span.TotalMilliseconds + "ms to execute - Rooms lagging behind");
        }
    }

    public IWiredItem LoadWiredBox(Item item)
    {
        var newBox = GenerateNewBox(item);
        DataRow row = null;
        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            dbClient.SetQuery("SELECT * FROM wired_items WHERE id=@id LIMIT 1");
            dbClient.AddParameter("id", item.Id);
            row = dbClient.GetRow();
            if (row != null)
            {
                if (string.IsNullOrEmpty(Convert.ToString(row["string"])))
                {
                    if (newBox.Type == WiredBoxType.ConditionMatchStateAndPosition || newBox.Type == WiredBoxType.ConditionDontMatchStateAndPosition)
                        newBox.StringData = "0;0;0";
                    else if (newBox.Type == WiredBoxType.ConditionUserCountInRoom || newBox.Type == WiredBoxType.ConditionUserCountDoesntInRoom)
                        newBox.StringData = "0;0";
                    else if (newBox.Type == WiredBoxType.ConditionFurniHasNoFurni)
                        newBox.StringData = "0";
                    else if (newBox.Type == WiredBoxType.EffectMatchPosition)
                        newBox.StringData = "0;0;0";
                    else if (newBox.Type == WiredBoxType.EffectMoveAndRotate)
                        newBox.StringData = "0;0";
                }
                newBox.StringData = Convert.ToString(row["string"]);
                newBox.BoolData = Convert.ToInt32(row["bool"]) == 1;
                newBox.ItemsData = Convert.ToString(row["items"]);
                if (newBox is IWiredCycle)
                {
                    var box = (IWiredCycle)newBox;
                    box.Delay = Convert.ToInt32(row["delay"]);
                }
                foreach (var str in Convert.ToString(row["items"]).Split(';'))
                {
                    var id = 0;
                    var sId = "0";
                    if (str.Contains(':'))
                        sId = str.Split(':')[0];
                    if (int.TryParse(str, out id) || int.TryParse(sId, out id))
                    {
                        var selectedItem = _room.GetRoomItemHandler().GetItem(Convert.ToInt32(id));
                        if (selectedItem == null)
                            continue;
                        newBox.SetItems.TryAdd(selectedItem.Id, selectedItem);
                    }
                }
            }
            else
            {
                newBox.ItemsData = "";
                newBox.StringData = "";
                newBox.BoolData = false;
                SaveBox(newBox);
            }
        }
        if (!AddBox(newBox))
        {
            // ummm
        }
        return newBox;
    }

    public IWiredItem GenerateNewBox(Item item)
    {
        switch (item.GetBaseItem().WiredType)
        {
            case WiredBoxType.TriggerRoomEnter:
                return new RoomEnterBox(_room, item);
            case WiredBoxType.TriggerRepeat:
                return new RepeaterBox(_room, item);
            case WiredBoxType.TriggerStateChanges:
                return new StateChangesBox(_room, item);
            case WiredBoxType.TriggerUserSays:
                return new UserSaysBox(_room, item);
            case WiredBoxType.TriggerWalkOffFurni:
                return new UserWalksOffBox(_room, item);
            case WiredBoxType.TriggerWalkOnFurni:
                return new UserWalksOnBox(_room, item);
            case WiredBoxType.TriggerGameStarts:
                return new GameStartsBox(_room, item);
            case WiredBoxType.TriggerGameEnds:
                return new GameEndsBox(_room, item);
            case WiredBoxType.TriggerUserFurniCollision:
                return new UserFurniCollision(_room, item);
            case WiredBoxType.TriggerUserSaysCommand:
                return new UserSaysCommandBox(_room, item);
            case WiredBoxType.EffectShowMessage:
                return new ShowMessageBox(_room, item);
            case WiredBoxType.EffectTeleportToFurni:
                return new TeleportUserBox(_room, item);
            case WiredBoxType.EffectToggleFurniState:
                return new ToggleFurniBox(_room, item);
            case WiredBoxType.EffectMoveAndRotate:
                return new MoveAndRotateBox(_room, item);
            case WiredBoxType.EffectKickUser:
                return new KickUserBox(_room, item);
            case WiredBoxType.EffectMuteTriggerer:
                return new MuteTriggererBox(_room, item);
            case WiredBoxType.EffectGiveReward:
                return new GiveRewardBox(_room, item);
            case WiredBoxType.EffectMatchPosition:
                return new MatchPositionBox(_room, item);
            case WiredBoxType.EffectAddActorToTeam:
                return new AddActorToTeamBox(_room, item);
            case WiredBoxType.EffectRemoveActorFromTeam:
                return new RemoveActorFromTeamBox(_room, item);
            /*
            
            case WiredBoxType.EffectMoveFurniToNearestUser:
                return new MoveFurniToNearestUserBox(_room, Item);
            case WiredBoxType.EffectMoveFurniFromNearestUser:
                return new MoveFurniFromNearestUserBox(_room, Item);

               */
            case WiredBoxType.ConditionFurniHasUsers:
                return new FurniHasUsersBox(_room, item);
            case WiredBoxType.ConditionTriggererOnFurni:
                return new TriggererOnFurniBox(_room, item);
            case WiredBoxType.ConditionTriggererNotOnFurni:
                return new TriggererNotOnFurniBox(_room, item);
            case WiredBoxType.ConditionFurniHasNoUsers:
                return new FurniHasNoUsersBox(_room, item);
            case WiredBoxType.ConditionFurniHasFurni:
                return new FurniHasFurniBox(_room, item);
            case WiredBoxType.ConditionIsGroupMember:
                return new IsGroupMemberBox(_room, item);
            case WiredBoxType.ConditionIsNotGroupMember:
                return new IsNotGroupMemberBox(_room, item);
            case WiredBoxType.ConditionUserCountInRoom:
                return new UserCountInRoomBox(_room, item);
            case WiredBoxType.ConditionUserCountDoesntInRoom:
                return new UserCountDoesntInRoomBox(_room, item);
            case WiredBoxType.ConditionIsWearingFx:
                return new IsWearingFxBox(_room, item);
            case WiredBoxType.ConditionIsNotWearingFx:
                return new IsNotWearingFxBox(_room, item);
            case WiredBoxType.ConditionIsWearingBadge:
                return new IsWearingBadgeBox(_room, item);
            case WiredBoxType.ConditionIsNotWearingBadge:
                return new IsNotWearingBadgeBox(_room, item);
            case WiredBoxType.ConditionMatchStateAndPosition:
                return new FurniMatchStateAndPositionBox(_room, item);
            case WiredBoxType.ConditionDontMatchStateAndPosition:
                return new FurniDoesntMatchStateAndPositionBox(_room, item);
            case WiredBoxType.ConditionFurniHasNoFurni:
                return new FurniHasNoFurniBox(_room, item);
            case WiredBoxType.ConditionActorHasHandItemBox:
                return new ActorHasHandItemBox(_room, item);
            case WiredBoxType.ConditionActorIsInTeamBox:
                return new ActorIsInTeamBox(_room, item);
            /*
            case WiredBoxType.ConditionMatchStateAndPosition:
                return new FurniMatchStateAndPositionBox(_room, Item);

            case WiredBoxType.ConditionFurniTypeMatches:
                return new FurniTypeMatchesBox(_room, Item);
            case WiredBoxType.ConditionFurniTypeDoesntMatch:
                return new FurniTypeDoesntMatchBox(_room, Item);
            case WiredBoxType.ConditionFurniHasNoFurni:
                return new FurniHasNoFurniBox(_room, Item);*/
            case WiredBoxType.AddonRandomEffect:
                return new AddonRandomEffectBox(_room, item);
            case WiredBoxType.EffectMoveFurniToNearestUser:
                return new MoveFurniToUserBox(_room, item);
            case WiredBoxType.EffectExecuteWiredStacks:
                return new ExecuteWiredStacksBox(_room, item);
            case WiredBoxType.EffectTeleportBotToFurniBox:
                return new TeleportBotToFurniBox(_room, item);
            case WiredBoxType.EffectBotChangesClothesBox:
                return new BotChangesClothesBox(_room, item);
            case WiredBoxType.EffectBotMovesToFurniBox:
                return new BotMovesToFurniBox(_room, item);
            case WiredBoxType.EffectBotCommunicatesToAllBox:
                return new BotCommunicatesToAllBox(_room, item);
            case WiredBoxType.EffectBotGivesHanditemBox:
                return new BotGivesHandItemBox(_room, item);
            case WiredBoxType.EffectBotFollowsUserBox:
                return new BotFollowsUserBox(_room, item);
            case WiredBoxType.EffectSetRollerSpeed:
                return new SetRollerSpeedBox(_room, item);
            case WiredBoxType.EffectRegenerateMaps:
                return new RegenerateMapsBox(_room, item);
            case WiredBoxType.EffectGiveUserBadge:
                return new GiveUserBadgeBox(_room, item);
        }
        return null;
    }

    public bool IsTrigger(Item item) => item.GetBaseItem().InteractionType == InteractionType.WiredTrigger;

    public bool IsEffect(Item item) => item.GetBaseItem().InteractionType == InteractionType.WiredEffect;

    public bool IsCondition(Item item) => item.GetBaseItem().InteractionType == InteractionType.WiredCondition;

    public bool OtherBoxHasItem(IWiredItem box, int itemId)
    {
        if (box == null)
            return false;
        ICollection<IWiredItem> items = GetEffects(box).Where(x => x.Item.Id != box.Item.Id).ToList();
        if (items != null && items.Count > 0)
        {
            foreach (var item in items)
            {
                if (item.Type != WiredBoxType.EffectMoveAndRotate && item.Type != WiredBoxType.EffectMoveFurniFromNearestUser && item.Type != WiredBoxType.EffectMoveFurniToNearestUser)
                    continue;
                if (item.SetItems == null || item.SetItems.Count == 0)
                    continue;
                if (item.SetItems.ContainsKey(itemId))
                    return true;
            }
        }
        return false;
    }

    public bool TriggerEvent(WiredBoxType type, params object[] @params)
    {
        var finished = false;
        try
        {
            if (type == WiredBoxType.TriggerUserSays)
            {
                var ranBoxes = new List<IWiredItem>();
                foreach (var box in _wiredItems.Values.ToList())
                {
                    if (box == null)
                        continue;
                    if (box.Type == WiredBoxType.TriggerUserSays)
                    {
                        if (!ranBoxes.Contains(box))
                            ranBoxes.Add(box);
                    }
                }
                var message = Convert.ToString(@params[1]);
                foreach (var box in ranBoxes.ToList())
                {
                    if (box == null)
                        continue;
                    if (message.Contains(" " + box.StringData) || message.Contains(box.StringData + " ") || message == box.StringData) finished = box.Execute(@params);
                }
                return finished;
            }
            foreach (var box in _wiredItems.Values.ToList())
            {
                if (box == null)
                    continue;
                if (box.Type == type && IsTrigger(box.Item)) finished = box.Execute(@params);
            }
        }
        catch
        {
            //log.Error("Error when triggering Wired Event: " + e);
            return false;
        }
        return finished;
    }

    public ICollection<IWiredItem> GetTriggers(IWiredItem item)
    {
        var items = new List<IWiredItem>();
        foreach (var I in _wiredItems.Values)
        {
            if (IsTrigger(I.Item) && I.Item.GetX == item.Item.GetX && I.Item.GetY == item.Item.GetY)
                items.Add(I);
        }
        return items;
    }

    public ICollection<IWiredItem> GetEffects(IWiredItem item)
    {
        var items = new List<IWiredItem>();
        foreach (var I in _wiredItems.Values)
        {
            if (IsEffect(I.Item) && I.Item.GetX == item.Item.GetX && I.Item.GetY == item.Item.GetY)
                items.Add(I);
        }
        return items.OrderBy(x => x.Item.GetZ).ToList();
    }

    public IWiredItem GetRandomEffect(ICollection<IWiredItem> effects)
    {
        return effects.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
    }

    public bool OnUserFurniCollision(Room room, Item item)
    {
        if (room == null || item == null)
            return false;
        foreach (var point in item.GetSides())
        {
            if (room.GetGameMap().SquareHasUsers(point.X, point.Y))
            {
                var users = room.GetGameMap().GetRoomUsers(point);
                if (users != null && users.Count > 0)
                {
                    foreach (var user in users.ToList())
                    {
                        if (user == null)
                            continue;
                        item.UserFurniCollision(user);
                    }
                }
                else
                    continue;
            }
            else
                continue;
        }
        return true;
    }

    public ICollection<IWiredItem> GetConditions(IWiredItem item)
    {
        var items = new List<IWiredItem>();
        foreach (var I in _wiredItems.Values)
        {
            if (IsCondition(I.Item) && I.Item.GetX == item.Item.GetX && I.Item.GetY == item.Item.GetY)
                items.Add(I);
        }
        return items;
    }

    public void OnEvent(Item item)
    {
        if (item.ExtraData == "1")
            return;
        item.ExtraData = "1";
        item.UpdateState(false, true);
        item.RequestUpdate(2, true);
    }

    public void SaveBox(IWiredItem item)
    {
        var items = "";
        IWiredCycle cycle = null;
        if (item is IWiredCycle) cycle = (IWiredCycle)item;
        foreach (var I in item.SetItems.Values)
        {
            var selectedItem = _room.GetRoomItemHandler().GetItem(Convert.ToInt32(I.Id));
            if (selectedItem == null)
                continue;
            if (item.Type == WiredBoxType.EffectMatchPosition || item.Type == WiredBoxType.ConditionMatchStateAndPosition || item.Type == WiredBoxType.ConditionDontMatchStateAndPosition)
                items += I.Id + ":" + I.GetX + "," + I.GetY + "," + I.GetZ + "," + I.Rotation + "," + I.ExtraData + ";";
            else
                items += I.Id + ";";
        }
        if (item.Type == WiredBoxType.EffectMatchPosition || item.Type == WiredBoxType.ConditionMatchStateAndPosition || item.Type == WiredBoxType.ConditionDontMatchStateAndPosition)
            item.ItemsData = items;
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("REPLACE INTO `wired_items` VALUES (@id, @items, @delay, @string, @bool)");
        dbClient.AddParameter("id", item.Item.Id);
        dbClient.AddParameter("items", items);
        dbClient.AddParameter("delay", item is IWiredCycle ? cycle.Delay : 0);
        dbClient.AddParameter("string", item.StringData);
        dbClient.AddParameter("bool", item.BoolData ? "1" : "0");
        dbClient.RunQuery();
    }

    public bool AddBox(IWiredItem item) => _wiredItems.TryAdd(item.Item.Id, item);

    public bool TryRemove(int itemId)
    {
        IWiredItem item = null;
        return _wiredItems.TryRemove(itemId, out item);
    }

    public bool TryGet(int id, out IWiredItem item) => _wiredItems.TryGetValue(id, out item);

    public void Cleanup()
    {
        _wiredItems.Clear();
    }
}
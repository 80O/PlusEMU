using Plus.Communication.Packets;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Inventory.Trading;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.Items;
using Plus.Core;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Inventory.Furniture;

namespace Plus.HabboHotel.Rooms.Trading;

public sealed class Trade
{
    private readonly Room _instance;
    private readonly TradeCompletion _completion = new();
    public object SyncRoot => _completion.SyncRoot;

    public Trade(int id, RoomUser playerOne, RoomUser playerTwo, Room room)
    {
        Id = id;
        CanChange = true;
        _instance = room;
        Users = new TradeUser[2];
        Users[0] = new(playerOne);
        Users[1] = new(playerTwo);
        playerOne.IsTrading = true;
        playerOne.TradeId = Id;
        playerOne.TradePartner = playerTwo.UserId;
        playerTwo.IsTrading = true;
        playerTwo.TradeId = Id;
        playerTwo.TradePartner = playerOne.UserId;
    }

    public int Id { get; set; }
    public TradeUser[] Users { get; set; }
    private bool _canChange;

    public bool CanChange
    {
        get => _canChange && !_completion.Closed;
        set => _canChange = value;
    }

    public bool AllAccepted
    {
        get
        {
            foreach (var user in Users)
            {
                if (user == null)
                    continue;
                if (!user.HasAccepted) return false;
            }

            return true;
        }
    }

    public void SendPacket(IServerPacket packet)
    {
        foreach (var user in Users)
        {
            if (user == null || user.RoomUser == null || user.RoomUser.GetClient() == null)
                continue;
            user.RoomUser.GetClient().Send(packet);
        }
    }

    public void RemoveAccepted()
    {
        foreach (var user in Users)
        {
            if (user == null)
                continue;
            user.HasAccepted = false;
        }
    }

    public void EndTrade(int userId)
    {
        lock (SyncRoot)
        {
            if (!_completion.TryCancel())
                return;
            CloseTrade();
            SendPacket(new TradingClosedComposer(userId));
        }
    }

    public void Finish()
    {
        lock (SyncRoot)
        {
            if (_completion.Closed || CanChange || !AllAccepted)
                return;
            try
            {
                var habboOne = Users[0].RoomUser.GetClient()?.GetHabbo();
                var habboTwo = Users[1].RoomUser.GetClient()?.GetHabbo();
                if (habboOne?.Inventory == null || habboTwo?.Inventory == null || habboOne.Id == habboTwo.Id)
                    throw new InvalidOperationException("Trade participant disconnected.");
                // Credit setters and disconnect saves use these locks too. Always acquire in user ID order.
                var first = habboOne.Id < habboTwo.Id ? habboOne : habboTwo;
                var second = habboOne.Id < habboTwo.Id ? habboTwo : habboOne;
                lock (first.Inventory.Furniture.SyncRoot)
                lock (second.Inventory.Furniture.SyncRoot)
                TradeCreditSynchronization.Run(habboOne, habboTwo, () =>
                {
                    var userOne = Users[0].OfferedItems.Values.ToArray();
                    var userTwo = Users[1].OfferedItems.Values.ToArray();
                    ValidateItems(userOne, habboOne, habboTwo);
                    ValidateItems(userTwo, habboTwo, habboOne);
                    var autoRedeem = PlusEnvironment.SettingsManager.TryGetValue("trading.auto_exchange_redeemables") ==
                                     "1";
                    var transfers = userOne.Select(i => CreateTransfer(i, habboOne.Id, habboTwo.Id, autoRedeem))
                        .Concat(userTwo.Select(i => CreateTransfer(i, habboTwo.Id, habboOne.Id, autoRedeem))).ToArray();
                    var creditsOne = checked(habboOne.Credits + userTwo
                        .Where(i => autoRedeem && i.Definition.InteractionType == InteractionType.Exchange)
                        .Sum(i => i.Definition.BehaviourData));
                    var creditsTwo = checked(habboTwo.Credits + userOne
                        .Where(i => autoRedeem && i.Definition.InteractionType == InteractionType.Exchange)
                        .Sum(i => i.Definition.BehaviourData));
                    var balances = new List<TradeCreditBalance>();
                    if (creditsOne != habboOne.Credits) balances.Add(new(habboOne.Id, creditsOne));
                    if (creditsTwo != habboTwo.Credits) balances.Add(new(habboTwo.Id, creditsTwo));

                    _completion.TryComplete(
                        () =>
                        {
                            using var connection = PlusEnvironment.DatabaseManager.Connection();
                            TradePersistence.Commit(connection, habboOne.Id, habboTwo.Id, transfers, balances);
                        },
                        () =>
                        {
                            habboOne.Credits = creditsOne;
                            habboTwo.Credits = creditsTwo;
                            ApplyItems(userOne, habboOne, habboTwo, autoRedeem);
                            ApplyItems(userTwo, habboTwo, habboOne, autoRedeem);
                        },
                        () =>
                        {
                            NotifyItems(userOne, habboOne, habboTwo, autoRedeem);
                            NotifyItems(userTwo, habboTwo, habboOne, autoRedeem);
                            foreach (var balance in balances)
                            {
                                var habbo = balance.UserId == habboOne.Id ? habboOne : habboTwo;
                                habbo.Client.Send(new CreditBalanceComposer(balance.Credits));
                            }

                            SendPacket(new TradingFinishComposer());
                        });
                });
            }
            catch (Exception exception)
            {
                ExceptionLogger.LogException(exception);
                _completion.TryCancel();
                // Reload persisted inventory/balances on next login, including an uncertain commit outcome.
                foreach (var user in Users)
                {
                    var client = user.RoomUser.GetClient();
                    client?.Disconnect();
                }
            }
            finally
            {
                CloseTrade();
            }
        }
    }

    private void CloseTrade()
    {
        foreach (var tradeUser in Users)
        {
            var roomUser = tradeUser.RoomUser;
            roomUser.RemoveStatus("trd");
            roomUser.UpdateNeeded = true;
            roomUser.IsTrading = false;
            roomUser.TradeId = 0;
            roomUser.TradePartner = 0;
        }

        _instance.GetTrading().RemoveTrade(Id);
    }

    private static TradeItemTransfer CreateTransfer(InventoryItem item, int from, int to, bool autoRedeem) =>
        new(item.Id, from, to, autoRedeem && item.Definition.InteractionType == InteractionType.Exchange);

    private static void ValidateItems(IEnumerable<InventoryItem> items, Habbo from, Habbo to)
    {
        foreach (var item in items)
        {
            if (!ReferenceEquals(from.Inventory.Furniture.GetItem(item.Id), item) ||
                to.Inventory.Furniture.HasItem(item.Id))
                throw new InvalidOperationException($"Trade item {item.Id} is no longer available.");
        }
    }

    private static void ApplyItems(IEnumerable<InventoryItem> items, Habbo from, Habbo to, bool autoRedeem)
    {
        foreach (var item in items)
        {
            if (!from.Inventory.Furniture.RemoveItem(item.Id))
                throw new InvalidOperationException("Committed trade inventory changed concurrently.");
            if (autoRedeem && item.Definition.InteractionType == InteractionType.Exchange)
                continue;
            item.OwnerId = (uint)to.Id;
            if (!to.Inventory.Furniture.AddItem(item))
                throw new InvalidOperationException("Committed trade item could not be added to inventory.");
        }
    }

    private static void NotifyItems(IEnumerable<InventoryItem> items, Habbo from, Habbo to, bool autoRedeem)
    {
        foreach (var item in items)
        {
            from.Client.Send(new FurniListRemoveComposer(item.Id));
            if (autoRedeem && item.Definition.InteractionType == InteractionType.Exchange)
                continue;
            to.Client.Send(new FurniListAddComposer(item));
            to.Client.Send(new FurniListNotificationComposer(item.Id, 1));
        }
    }
}

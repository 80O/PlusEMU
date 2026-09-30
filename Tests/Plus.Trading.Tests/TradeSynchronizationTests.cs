using Plus.HabboHotel.Rooms.Trading;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Inventory.Furniture;
using Xunit;

namespace Plus.Trading.Tests;

public class TradeSynchronizationTests
{
    private static Habbo User(int id) => new()
    {
        Id = id,
        Credits = 100,
        HabboStats = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "", 0)
    };

    [Fact]
    public async Task CreditAdjustmentWaitsForTradeAndUsesItsCommittedBalance()
    {
        var one = User(1);
        var two = User(2);
        using var started = new ManualResetEventSlim();
        Task? adjustment = null;
        TradeCreditSynchronization.Run(one, two, () =>
        {
            adjustment = Task.Run(() => { started.Set(); one.AdjustCredits(1); });
            Assert.True(started.Wait(TimeSpan.FromSeconds(5)));
            Assert.False(adjustment.IsCompleted);
            one.Credits = 150;
        });
        // Await the worker outside the trade locks.
        await adjustment!;
        Assert.Equal(151, one.Credits);
    }

    [Fact]
    public async Task UnknownCommitMarksBothBalancesBeforeWaitingSaveCanRun()
    {
        var one = User(1);
        var two = User(2);
        using var started = new ManualResetEventSlim();
        Task<string[]>? save = null;
        Assert.Throws<TradeCommitOutcomeUnknownException>(() => TradeCreditSynchronization.Run(two, one, () =>
        {
            save = Task.Run(() =>
            {
                started.Set();
                lock (one.CreditsSyncRoot)
                lock (two.CreditsSyncRoot)
                    return new[] { one.GetQueryString, two.GetQueryString };
            });
            Assert.True(started.Wait(TimeSpan.FromSeconds(5)));
            Assert.False(save.IsCompleted);
            throw new TradeCommitOutcomeUnknownException(new IOException("Injected lost commit acknowledgement"));
        }));
        foreach (var query in await save!)
        {
            Assert.DoesNotContain("`credits` =", query);
            Assert.Contains("`vip_points` =", query);
        }
    }

    [Fact]
    public void OrdinaryFailureDoesNotSuppressCreditSaves()
    {
        var one = User(1);
        var two = User(2);
        Assert.Throws<InvalidOperationException>(() => TradeCreditSynchronization.Run(one, two,
            () => throw new InvalidOperationException("Injected rolled-back write")));
        Assert.Contains("`credits` = '100'", one.GetQueryString);
        Assert.Contains("`credits` = '100'", two.GetQueryString);
    }

    [Fact]
    public void ConcurrentCreditAdjustmentsDoNotLoseUpdates()
    {
        var one = User(1);
        Parallel.For(0, 1000, _ => one.AdjustCredits(1));
        Assert.Equal(1100, one.Credits);
    }

    [Fact]
    public async Task ItemRemovalCannotRunInsideTradeInventoryCriticalSection()
    {
        var item = new InventoryItem { Id = 10 };
        var inventory = new FurnitureInventoryComponent(new[] { item }, Array.Empty<InventoryItem>());
        using var started = new ManualResetEventSlim();
        Task<bool> removal;
        lock (inventory.SyncRoot)
        {
            removal = Task.Run(() => { started.Set(); return inventory.RemoveItem(10); });
            Assert.True(started.Wait(TimeSpan.FromSeconds(5)));
            Assert.False(removal.IsCompleted);
            Assert.Same(item, inventory.GetItem(10));
        }
        Assert.True(await removal);
        Assert.Null(inventory.GetItem(10));
    }
}

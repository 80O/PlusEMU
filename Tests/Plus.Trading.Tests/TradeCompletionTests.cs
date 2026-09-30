using Plus.HabboHotel.Rooms.Trading;
using Xunit;

namespace Plus.Trading.Tests;

public class TradeCompletionTests
{
    [Fact]
    public void NotificationsFollowPersistenceAndInventoryChanges()
    {
        var completion = new TradeCompletion();
        var operations = new List<string>();
        Assert.True(completion.TryComplete(() => operations.Add("commit"),
            () => operations.Add("inventory"), () => operations.Add("success")));
        Assert.Equal(new[] { "commit", "inventory", "success" }, operations);
    }

    [Fact]
    public void FailedCommitDoesNotApplyOrNotifyAndCannotBeRetried()
    {
        var completion = new TradeCompletion();
        var changed = false;
        Assert.Throws<InvalidOperationException>(() => completion.TryComplete(
            () => throw new InvalidOperationException(), () => changed = true, () => changed = true));
        Assert.False(changed);
        Assert.False(completion.TryComplete(() => changed = true, () => changed = true, () => changed = true));
        Assert.False(changed);
    }

    [Fact]
    public void FailedInventoryApplicationDoesNotReportSuccess()
    {
        var completion = new TradeCompletion();
        var notified = false;
        Assert.Throws<InvalidOperationException>(() => completion.TryComplete(() => { },
            () => throw new InvalidOperationException(), () => notified = true));
        Assert.False(notified);
        Assert.True(completion.Closed);
    }

    [Fact]
    public void ConcurrentConfirmationsCommitOnlyOnce()
    {
        var completion = new TradeCompletion();
        var commits = 0;
        var notifications = 0;
        Parallel.For(0, 100, _ => completion.TryComplete(
            () => Interlocked.Increment(ref commits), () => { }, () => Interlocked.Increment(ref notifications)));
        Assert.Equal(1, commits);
        Assert.Equal(1, notifications);
    }

    [Fact]
    public void CancellationPreventsCompletion()
    {
        var completion = new TradeCompletion();
        Assert.True(completion.TryCancel());
        Assert.False(completion.TryCancel());
        Assert.False(completion.TryComplete(() => throw new Exception(), () => throw new Exception(), () => throw new Exception()));
    }

    [Fact]
    public void ConcurrentCancellationAndConfirmationHaveOnlyOneWinner()
    {
        var completion = new TradeCompletion();
        var winners = 0;
        Parallel.Invoke(
            () => { if (completion.TryCancel()) Interlocked.Increment(ref winners); },
            () => { if (completion.TryComplete(() => { }, () => { }, () => { })) Interlocked.Increment(ref winners); });
        Assert.Equal(1, winners);
    }
}

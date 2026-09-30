using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Trading;

public static class TradeCreditSynchronization
{
    public static void Run(Habbo userOne, Habbo userTwo, Action settle)
    {
        var first = userOne.Id < userTwo.Id ? userOne : userTwo;
        var second = userOne.Id < userTwo.Id ? userTwo : userOne;
        lock (first.CreditsSyncRoot)
        lock (second.CreditsSyncRoot)
        {
            try
            {
                settle();
            }
            catch (TradeCommitOutcomeUnknownException)
            {
                // Mark both balances before disconnect/shutdown saves can acquire either lock.
                userOne.RequireCreditsReload();
                userTwo.RequireCreditsReload();
                throw;
            }
        }
    }
}

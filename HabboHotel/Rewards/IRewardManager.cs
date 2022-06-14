using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rewards;

public interface IRewardManager
{
    void Init();
    Task CheckRewards(GameClient session);
}
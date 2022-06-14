using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database;
using Plus.HabboHotel.Badges;
using Plus.HabboHotel.GameClients;
using System.Collections.Concurrent;
using System.Data;

namespace Plus.HabboHotel.Rewards;

public class RewardManager : IRewardManager
{
    private readonly IDatabase _database;
    private readonly IBadgeManager _badgeManager;
    private readonly ConcurrentDictionary<int, List<int>> _rewardLogs;
    private readonly ConcurrentDictionary<int, Reward> _rewards;

    public RewardManager(IDatabase database, IBadgeManager badgeManager)
    {
        _database = database;
        _badgeManager = badgeManager;
        _rewards = new ConcurrentDictionary<int, Reward>();
        _rewardLogs = new ConcurrentDictionary<int, List<int>>();
    }

    public void Init()
    {
        using var dbClient = _database.GetQueryReactor();
        dbClient.SetQuery("SELECT * FROM `server_rewards` WHERE enabled = '1'");
        var dTable = dbClient.GetTable();
        if (dTable != null)
        {
            foreach (DataRow dRow in dTable.Rows)
            {
                _rewards.TryAdd((int)dRow["id"],
                    new Reward(Convert.ToDouble(dRow["reward_start"]), Convert.ToDouble(dRow["reward_end"]), Convert.ToString(dRow["reward_type"]), Convert.ToString(dRow["reward_data"]),
                        Convert.ToString(dRow["message"])));
            }
        }
        dbClient.SetQuery("SELECT * FROM `server_reward_logs`");
        dTable = dbClient.GetTable();
        if (dTable != null)
        {
            foreach (DataRow dRow in dTable.Rows)
            {
                var id = (int)dRow["user_id"];
                var rewardId = (int)dRow["reward_id"];
                if (!_rewardLogs.ContainsKey(id))
                    _rewardLogs.TryAdd(id, new List<int>());
                if (!_rewardLogs[id].Contains(rewardId))
                    _rewardLogs[id].Add(rewardId);
            }
        }
    }

    private bool HasReward(int id, int rewardId)
    {
        if (!_rewardLogs.ContainsKey(id))
            return false;
        if (_rewardLogs[id].Contains(rewardId))
            return true;
        return false;
    }

    private void LogReward(int id, int rewardId)
    {
        if (!_rewardLogs.ContainsKey(id))
            _rewardLogs.TryAdd(id, new List<int>());
        if (!_rewardLogs[id].Contains(rewardId))
            _rewardLogs[id].Add(rewardId);
        using var dbClient = _database.GetQueryReactor();
        dbClient.SetQuery("INSERT INTO `server_reward_logs` VALUES ('', @userId, @rewardId)");
        dbClient.AddParameter("userId", id);
        dbClient.AddParameter("rewardId", rewardId);
        dbClient.RunQuery();
    }

    public async Task CheckRewards(GameClient session)
    {
        if (session == null || session.GetHabbo() == null)
            return;
        foreach (var entry in _rewards)
        {
            var id = entry.Key;
            var reward = entry.Value;
            if (HasReward(session.GetHabbo().Id, id))
                continue;
            if (reward.Active)
            {
                switch (reward.Type)
                {
                    case RewardType.Badge:
                        {
                            if (!session.GetHabbo().Inventory.Badges.HasBadge(reward.RewardData))
                                await _badgeManager.GiveBadge(session.GetHabbo(), reward.RewardData);
                            break;
                        }
                    case RewardType.Credits:
                        {
                            session.GetHabbo().Credits += Convert.ToInt32(reward.RewardData);
                            session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
                            break;
                        }
                    case RewardType.Duckets:
                        {
                            session.GetHabbo().Duckets += Convert.ToInt32(reward.RewardData);
                            session.SendPacket(new HabboActivityPointNotificationComposer(session.GetHabbo().Duckets, Convert.ToInt32(reward.RewardData)));
                            break;
                        }
                    case RewardType.Diamonds:
                        {
                            session.GetHabbo().Diamonds += Convert.ToInt32(reward.RewardData);
                            session.SendPacket(new HabboActivityPointNotificationComposer(session.GetHabbo().Diamonds, Convert.ToInt32(reward.RewardData), 5));
                            break;
                        }
                }
                if (!string.IsNullOrEmpty(reward.Message))
                    session.SendNotification(reward.Message);
                LogReward(session.GetHabbo().Id, id);
            }
            else
                continue;
        }
    }
}